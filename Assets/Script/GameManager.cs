using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour,ISaveable
{
  public static GameManager Instance;
    private Vector3 lastPlayerPosition;
    private bool hasForcedRespawnPoint;
    private Vector3 forcedRespawnPoint;

    private string lastScenePlayed;
    private bool dataLoaded;
    [Header("Scene Transition")]
    [SerializeField] private float fadeOutDuration = 1.2f;
    [SerializeField] private float fadeInDuration = 1.4f;
    [SerializeField] private float postLoadBlackScreenDuration = 0.12f;
    [SerializeField] private int postTeleportSettleFrames = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //public void SetLastPlayerPosition(Vector3 position)
    //{
    //    lastPlayerPosition = position;
    //}

    public void ContinuePlay()
    {
        ChangeScene(lastScenePlayed,RespawnType.None);
    }

    public void StartNewGame(string sceneName)
    {
        hasForcedRespawnPoint = false;
        Time.timeScale = 1f;
        StartCoroutine(ChangeSceneCo(sceneName, RespawnType.None));
    }

    public void RestartScene()
    {
        hasForcedRespawnPoint = false;
        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName,RespawnType.None);
    }

    public void RestartSceneAt(Vector3 respawnPosition)
    {
        forcedRespawnPoint = respawnPosition;
        hasForcedRespawnPoint = true;

        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName, RespawnType.None);
    }

    public void ChangeScene(string sceneName,RespawnType respawnType)
    {
        if (respawnType != RespawnType.None)
            hasForcedRespawnPoint = false;

        SaveManager.instance.SaveGame();
        Time.timeScale = 1f;
        StartCoroutine(ChangeSceneCo(sceneName,respawnType));
    }

    private IEnumerator ChangeSceneCo(string sceneName,RespawnType respawnType)
    {
        UI_FadeScreen fadeScreenUI = FindFadeScreenUI();


        fadeScreenUI.FadeOut(fadeOutDuration);

        yield return fadeScreenUI.fadeEffectCo;

        SceneManager.LoadScene(sceneName);

        dataLoaded = false;
        yield return null;

       while(!dataLoaded)
            yield return null;

       fadeScreenUI=FindFadeScreenUI();
        if (fadeScreenUI != null)
            fadeScreenUI.SetBlackScreen(true);

        if (postLoadBlackScreenDuration > 0f)
            yield return new WaitForSecondsRealtime(postLoadBlackScreenDuration);

        Player player = Player.instance;

        if (player != null)
        {
            Vector3 position = GetNewPlayerPosition(respawnType);

            if(position != Vector3.zero)
            {
                player.TeleportPlayer(position);
                player.ZeroVelocity();
                Physics2D.SyncTransforms();
            }

            for (int i = 0; i < Mathf.Max(0, postTeleportSettleFrames); i++)
                yield return new WaitForEndOfFrame();
        }

        if (fadeScreenUI != null)
            fadeScreenUI.FadeIn(fadeInDuration);
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI.instance != null)
            return UI.instance.fadeScreenUI;
        else
            return FindFirstObjectByType<UI_FadeScreen>();

    }


    private Vector3 GetNewPlayerPosition(RespawnType type)
    {
        if (type == RespawnType.None && hasForcedRespawnPoint)
        {
            hasForcedRespawnPoint = false;
            return forcedRespawnPoint;
        }

        if(type == RespawnType.Portal)
        {
            Object_Portal portal = Object_Portal.instance;

            Vector3 position = portal.GetPosition();

            portal.SetTrigger(false);
            portal.DisableIfNeeded();

            return position;
        }


        if(type == RespawnType.None)
        {
            var data = SaveManager.instance.GetGameData();
            if (string.IsNullOrWhiteSpace(data.currentCheckpointId) == false)
            {
                var currentCheckpoint = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None)
                    .FirstOrDefault(cp => cp.GetCheckPointId() == data.currentCheckpointId);

                if (currentCheckpoint != null)
                    return currentCheckpoint.GetPosition();
            }

            var checkpoints = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None);
            var unlockedCheckPoints = checkpoints.Where(cp => data.unlockedCheckpoints.TryGetValue(cp.GetCheckPointId(), out bool unlocked) && unlocked).Select(cp=>cp.GetPosition()).ToList();

            var enterWaypoints=FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None).Where(wp=>wp.GetWaypointType()==RespawnType.Enter).Select(wp=>wp.GetPositionAndSetTriggerFalse()).ToList();

            var selectedPositions = unlockedCheckPoints.Concat(enterWaypoints).ToList();

            if(selectedPositions.Count==0)
                return Vector3.zero;

            return selectedPositions.OrderBy(position => Vector3.Distance(position, lastPlayerPosition)).First();

        }
        return GetWaypointPosition(type);
    }
    private Vector3 GetWaypointPosition(RespawnType type)
    {
        var waypoints = FindObjectsByType<Object_Waypoint>(FindObjectsSortMode.None);

        foreach(var point in waypoints)
        {
            if (point.GetWaypointType() == type)
            {
               
                return point.GetPositionAndSetTriggerFalse();
            }
        }
        return Vector3.zero;
    }

    public void LoadData(GameData data)
    {
        lastScenePlayed = data.lastScenePlayed;
        lastPlayerPosition = data.lastPlayerPosition;

        if (string.IsNullOrEmpty(lastScenePlayed))
            lastScenePlayed = "Level_0";


        dataLoaded = true;
    }

    public void SaveData(ref GameData data)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if(currentScene == "MainMenu")
        {
            return;
        }

        data.lastPlayerPosition = Player.instance.transform.position;
        data.lastScenePlayed = currentScene;
        dataLoaded = false;
    }
}
