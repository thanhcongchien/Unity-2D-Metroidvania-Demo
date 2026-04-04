using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Object_CheckPoint : MonoBehaviour, ISaveable, IInteractable
{
    [SerializeField] private string checkPointId;
    [SerializeField] private Transform respawnPoint;

    [Header("Interact Tooltip")]
    [SerializeField] private GameObject interactToolTip;
    [SerializeField] private float floatSpeed = 8f;
    [SerializeField] private float floatRange = .1f;

    public bool isActive { get; private set; }
    private Animator anim;
    private AudioSource fireAudio;
    private Transform player;
    private Vector3 toolTipStartPos;
    private bool restRequested;
    private string runtimeCheckPointId;

    private void Awake()
    {
        EnsureCheckpointSaveId();
        anim = GetComponentInChildren<Animator>();
        fireAudio = GetComponent<AudioSource>();

        if (fireAudio != null && fireAudio.clip != null)
            fireAudio.clip.LoadAudioData();

        if (interactToolTip != null)
        {
            toolTipStartPos = interactToolTip.transform.position;
            interactToolTip.SetActive(false);
        }
    }
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(checkPointId))
        {
            checkPointId = System.Guid.NewGuid().ToString();
        }
#endif

    }

    private void Update()
    {
        HandleToolTipFloat();
    }

    private void HandleToolTipFloat()
    {
        if (interactToolTip == null || interactToolTip.activeSelf == false)
            return;

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        interactToolTip.transform.position = toolTipStartPos + new Vector3(0f, yOffset, 0f);
    }

    public string GetCheckPointId()
    {
        EnsureCheckpointSaveId();
        return runtimeCheckPointId;
    }
    public Vector3 GetPosition()
    {
        return respawnPoint == null ? transform.position : respawnPoint.position;
    }
    public void ActivateCheckPoint(bool activate)
    {
        isActive = activate;
        anim.SetBool("isActive", activate);

        if (isActive && fireAudio != null && fireAudio.isPlaying == false)
        {
            fireAudio.Play();

        }
        if (!isActive && fireAudio != null)
            fireAudio.Stop();


    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TryGetPlayerTransform(collision, out Transform playerTransform) == false)
            return;

        player = playerTransform;
        SetAsCurrentCheckpoint();

        if (interactToolTip != null)
        {
            toolTipStartPos = interactToolTip.transform.position;
            interactToolTip.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (TryGetPlayerTransform(collision, out Transform playerTransform) == false)
            return;

        if (player == playerTransform)
            player = null;

        if (interactToolTip != null)
            interactToolTip.SetActive(false);
    }

    public void Interact()
    {
        if (restRequested)
            return;

        if (player == null || Player.instance == null || GameManager.Instance == null)
            return;
        if (isActive == false)
            return;
        if (interactToolTip != null && interactToolTip.activeSelf == false)
            return;
        if (HasHigherPriorityInteractableNearby(Player.instance.transform.position))
            return;

        restRequested = true;

        Player.instance.health.SetHealthToPercent(1f);

        if (interactToolTip != null)
            interactToolTip.SetActive(false);

        GameManager.Instance.RestartSceneAt(GetPosition());
    }

    private bool HasHigherPriorityInteractableNearby(Vector3 playerPosition)
    {
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(playerPosition, 1f);
        foreach (var target in objectsAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = target.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            if (interactable is Object_CheckPoint)
                continue;

            return true;
        }

        return false;
    }

    private bool TryGetPlayerTransform(Collider2D collision, out Transform playerTransform)
    {
        playerTransform = null;
        if (collision == null)
            return false;

        Player playerComponent = collision.GetComponentInParent<Player>();
        if (playerComponent == null)
            return false;

        playerTransform = playerComponent.transform;
        return true;
    }

    private void SetAsCurrentCheckpoint()
    {
        Object_CheckPoint[] checkpoints = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None);

        foreach (var checkpoint in checkpoints)
        {
            checkpoint.ActivateCheckPoint(checkpoint == this);
        }
    }

    private void EnsureCheckpointSaveId()
    {
        if (string.IsNullOrWhiteSpace(runtimeCheckPointId) == false)
            return;

        runtimeCheckPointId = BuildCheckpointSaveId();
    }

    private string BuildCheckpointSaveId()
    {
        List<string> hierarchyPath = new List<string>();
        Transform current = transform;

        while (current != null)
        {
            hierarchyPath.Add($"{current.GetSiblingIndex()}:{current.name}");
            current = current.parent;
        }

        hierarchyPath.Reverse();

        string sceneId = string.IsNullOrWhiteSpace(gameObject.scene.path) ? gameObject.scene.name : gameObject.scene.path;
        return $"{sceneId}/{string.Join("/", hierarchyPath)}";
    }

    private void EnsureCheckpointSaveData(GameData data)
    {
        if (data.unlockedCheckpoints == null)
            data.unlockedCheckpoints = new SerializableDictionary<string, bool>();
    }

    private bool HasLegacyCheckpointId()
    {
        return string.IsNullOrWhiteSpace(checkPointId) == false && checkPointId != runtimeCheckPointId;
    }

    private bool IsBestLegacyCheckpointCandidate(GameData data)
    {
        if (HasLegacyCheckpointId() == false)
            return false;

        Object_CheckPoint closestLegacyCheckpoint = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None)
            .Where(cp => cp.checkPointId == checkPointId)
            .OrderBy(cp => Vector3.Distance(cp.GetPosition(), data.lastPlayerPosition))
            .FirstOrDefault();

        return closestLegacyCheckpoint == this;
    }

    public void LoadData(GameData data)
    {
        EnsureCheckpointSaveId();
        EnsureCheckpointSaveData(data);

        restRequested = false;
        bool shouldBeActive = false;

        if (string.IsNullOrWhiteSpace(data.currentCheckpointId) == false)
        {
            if (data.currentCheckpointId == runtimeCheckPointId)
            {
                shouldBeActive = true;
            }
            else if (HasLegacyCheckpointId() && data.currentCheckpointId == checkPointId && IsBestLegacyCheckpointCandidate(data))
            {
                shouldBeActive = true;
                data.currentCheckpointId = runtimeCheckPointId;
            }
        }
        else if (data.unlockedCheckpoints.TryGetValue(runtimeCheckPointId, out bool runtimeActive) && runtimeActive)
        {
            shouldBeActive = true;
            data.currentCheckpointId = runtimeCheckPointId;
        }
        else if (HasLegacyCheckpointId()
            && data.unlockedCheckpoints.TryGetValue(checkPointId, out bool legacyActive)
            && legacyActive
            && IsBestLegacyCheckpointCandidate(data))
        {
            shouldBeActive = true;
            data.unlockedCheckpoints[runtimeCheckPointId] = true;
            data.currentCheckpointId = runtimeCheckPointId;
        }

        ActivateCheckPoint(shouldBeActive);
        data.unlockedCheckpoints[runtimeCheckPointId] = shouldBeActive;

        if (HasLegacyCheckpointId())
            data.unlockedCheckpoints[checkPointId] = false;
    }

    public void SaveData(ref GameData data)
    {
        EnsureCheckpointSaveId();
        EnsureCheckpointSaveData(data);
        data.unlockedCheckpoints[runtimeCheckPointId] = isActive;

        if (HasLegacyCheckpointId())
            data.unlockedCheckpoints[checkPointId] = false;

        if (isActive)
            data.currentCheckpointId = runtimeCheckPointId;
        else if (data.currentCheckpointId == runtimeCheckPointId || (HasLegacyCheckpointId() && data.currentCheckpointId == checkPointId))
            data.currentCheckpointId = string.Empty;
    }
}
