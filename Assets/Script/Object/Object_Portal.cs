using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_Portal : MonoBehaviour, ISaveable
{
    private static readonly Vector3 HiddenPosition = new Vector3(9999f, 9999f, 0f);

    public static Object_Portal instance;
    public bool isActive { get; private set; }

    [SerializeField] private Vector2 defaultPosition;
    [SerializeField] private string townSceneName = "Level_0";

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered;

    private string currentSceneName;
    private string returnSceneName;
    private bool returningFromTown;
    private Vector3 sceneDefaultPosition;

    private void Awake()
    {
        instance = this;
        currentSceneName = SceneManager.GetActiveScene().name;
        sceneDefaultPosition = transform.position;
        transform.position = HiddenPosition;
    }

    public void ActivatePortal(Vector3 position, int facingDir = 1)
    {
        isActive = true;
        transform.position = position;
        SaveManager.instance.GetGameData().inScenePortals.Clear();

        if (facingDir == -1)
        {
            transform.Rotate(0, 180, 0);
        }
    }

    public void DisableIfNeeded()
    {
        if (returningFromTown == false)
            return;

        SaveManager.instance.GetGameData().inScenePortals.Remove(currentSceneName);
        isActive = false;
        transform.position = HiddenPosition;
    }

    private void UseTeleport()
    {
        string destinationScene = InTown() ? returnSceneName : townSceneName;
        GameManager.Instance.ChangeScene(destinationScene, RespawnType.Portal);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBeTriggered)
            return;

        Player player = collision.GetComponentInParent<Player>();
        if (player == null)
            return;

        UseTeleport();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        canBeTriggered = true;
    }

    public void SetTrigger(bool trigger) => canBeTriggered = trigger;

    public Vector3 GetPosition() => respawnPoint != null ? respawnPoint.position : transform.position;

    private bool InTown() => currentSceneName == townSceneName;

    public void LoadData(GameData data)
    {
        isActive = false;
        transform.position = HiddenPosition;

        if (InTown() && data.inScenePortals.Count > 0)
        {
            transform.position = GetPortalSpawnPosition();
            isActive = true;
        }
        else if (data.inScenePortals.TryGetValue(currentSceneName, out Vector3 portalPosition))
        {
            transform.position = portalPosition;
            isActive = true;
        }

        returningFromTown = data.returningFromTown;
        returnSceneName = data.portalDestinationSceneName;
    }

    public void SaveData(ref GameData data)
    {
        data.returningFromTown = InTown();
        if (isActive && InTown() == false)
        {
            data.inScenePortals[currentSceneName] = transform.position;
            data.portalDestinationSceneName = currentSceneName;
        }
        else
        {
            data.inScenePortals.Remove(currentSceneName);
        }
    }

    private Vector3 GetPortalSpawnPosition()
    {
        Vector3 configuredPosition = new Vector3(defaultPosition.x, defaultPosition.y, sceneDefaultPosition.z);

        // Fall back to the scene placement when no explicit spawn position is configured.
        if (defaultPosition == Vector2.zero)
            return sceneDefaultPosition;

        return configuredPosition;
    }
}
