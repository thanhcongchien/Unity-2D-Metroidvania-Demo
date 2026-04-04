using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;
    
    
    [SerializeField] private string fileName = "FirstTake.json";
    [SerializeField] private bool enctryptData = true;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }


    private IEnumerator Start()
    {
        Debug.Log(Application.persistentDataPath);
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName,enctryptData);
        RefreshSaveables();

        yield return null;
        LoadGame();
    }
    public GameData GetGameData()
    {
        return gameData;
    }
    private void LoadGame()
    {
        RefreshSaveables();
        gameData = dataHandler.LoadData();

        if(gameData == null)
        {
            Debug.Log("No data to load");
            gameData = new GameData();
            return;
        }

        foreach(var saveable in allSaveables)
        {
            saveable.LoadData(gameData);
        }
    }


    public void SaveGame()
    {
        if (gameData == null)
            gameData = new GameData();

        RefreshSaveables();

        foreach(var saveable in allSaveables)
        {
            saveable.SaveData(ref gameData);
        }

        dataHandler.SaveData(gameData);

    }

    [ContextMenu("*** Delete save data ***")]
    public void DeleteSaveData()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName,enctryptData);
        dataHandler.Delete();

        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private List<ISaveable> FindIsaveables()
    {
        return 
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).
            OfType<ISaveable>().ToList();
    }

    private void RefreshSaveables()
    {
        allSaveables = FindIsaveables();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSaveables();
    }

}
