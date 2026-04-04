using System.Collections;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ReaperBossIntroTrigger : MonoBehaviour
{
    [Header("Boss Intro")]
    [SerializeField] private Enemy_Reaper reaper;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform focusTarget;
    [SerializeField] private Vector3 autoFocusOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float focusDelay = .15f;
    [SerializeField] private float focusHoldDuration = 1.35f;
    [SerializeField] private float returnDelay = .15f;
    [SerializeField] private bool freezePlayerDuringIntro = true;
    [SerializeField] private bool lockReaperUntilIntro = true;
    [SerializeField] private bool startBattleAfterIntro = true;
    [SerializeField] private bool startBossMusicOnTrigger = true;
    [SerializeField] private bool disableTriggerAfterUse = true;

    [Header("Camera Damping")]
    [SerializeField] private bool overrideDampingDuringIntro = true;
    [SerializeField] private float introXDamping = .35f;
    [SerializeField] private float introYDamping = .35f;

    private bool hasTriggered;
    private bool cameraStateCached;
    private Transform originalFollow;
    private float originalXDamping;
    private float originalYDamping;
    private Transform generatedFocusTarget;
    private CinemachineFramingTransposer framingTransposer;

    private IEnumerator Start()
    {
        ResolveDependencies();
        yield return null;

        if (lockReaperUntilIntro && reaper != null)
            reaper.SetBattleUnlocked(false);
    }

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        Player player = collision.GetComponentInParent<Player>();
        if (player == null)
            return;

        hasTriggered = true;
        StartCoroutine(PlayBossIntro(player));
    }

    private IEnumerator PlayBossIntro(Player player)
    {
        ResolveDependencies();

        if (reaper == null || virtualCamera == null || player == null)
        {
            RestorePlayerControl(player);
            yield break;
        }

        if (startBossMusicOnTrigger)
            reaper.StartBossBattleMusic();

        if (freezePlayerDuringIntro)
        {
            player.ZeroVelocity();
            player.SetGameplayInputEnabled(false);
        }

        if (focusDelay > 0f)
            yield return new WaitForSeconds(focusDelay);

        CacheCameraState();
        ApplyIntroCameraSettings();
        virtualCamera.Follow = ResolveFocusTarget();

        if (focusHoldDuration > 0f)
            yield return new WaitForSeconds(focusHoldDuration);

        RestoreCameraState();

        if (returnDelay > 0f)
            yield return new WaitForSeconds(returnDelay);

        if (startBattleAfterIntro)
            reaper.BeginBossEncounter(player.transform);
        else if (lockReaperUntilIntro)
            reaper.SetBattleUnlocked(true);

        RestorePlayerControl(player);

        if (disableTriggerAfterUse)
            gameObject.SetActive(false);
    }

    private void ResolveDependencies()
    {
        if (reaper == null)
            reaper = FindFirstObjectByType<Enemy_Reaper>();

        if (virtualCamera == null)
            virtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();

        if (virtualCamera != null && framingTransposer == null)
            framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void CacheCameraState()
    {
        if (cameraStateCached || virtualCamera == null)
            return;

        originalFollow = virtualCamera.Follow;

        if (framingTransposer != null)
        {
            originalXDamping = framingTransposer.m_XDamping;
            originalYDamping = framingTransposer.m_YDamping;
        }

        cameraStateCached = true;
    }

    private void ApplyIntroCameraSettings()
    {
        if (overrideDampingDuringIntro == false || framingTransposer == null)
            return;

        framingTransposer.m_XDamping = Mathf.Max(0f, introXDamping);
        framingTransposer.m_YDamping = Mathf.Max(0f, introYDamping);
    }

    private void RestoreCameraState()
    {
        if (virtualCamera == null)
            return;

        if (cameraStateCached)
            virtualCamera.Follow = originalFollow;

        if (framingTransposer != null && overrideDampingDuringIntro)
        {
            framingTransposer.m_XDamping = originalXDamping;
            framingTransposer.m_YDamping = originalYDamping;
        }
    }

    private Transform ResolveFocusTarget()
    {
        if (focusTarget != null)
            return focusTarget;

        if (reaper == null)
            return originalFollow;

        if (generatedFocusTarget == null)
        {
            GameObject focusAnchor = new GameObject("ReaperIntroFocusTarget");
            generatedFocusTarget = focusAnchor.transform;
            generatedFocusTarget.SetParent(reaper.transform, false);
            generatedFocusTarget.localPosition = autoFocusOffset;
        }

        return generatedFocusTarget;
    }

    private void RestorePlayerControl(Player player)
    {
        if (player == null || freezePlayerDuringIntro == false)
            return;

        player.ZeroVelocity();
        player.SetGameplayInputEnabled(true);
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        BoxCollider2D box = trigger as BoxCollider2D;
        if (box == null)
            return;

        Gizmos.color = new Color(0.8f, 0.15f, 0.15f, 0.35f);
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.offset, box.size);
        Gizmos.matrix = previousMatrix;
    }
}
