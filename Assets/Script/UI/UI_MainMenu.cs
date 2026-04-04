using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private string newGameStartScene = "Level_0";
   
    private void Start()
    {
        transform.root.GetComponentInChildren<UI_Options>(true).LoadUpVolume();

        transform.root.GetComponentInChildren<UI_FadeScreen>().FadeIn();
        AudioManager.instance.StartBGM("playlist_mainmenu");
        RegisterButtonClickSFX();
    }
    public void PlayBTN()
    {
        StartNewGame();
    }

    public void ContinueBTN()
    {
        GameManager.Instance.ContinuePlay();
    }

    public void QuitBTN()
    {
        Application.Quit();
    }

    private void RegisterButtonClickSFX()
    {
        Button[] buttons = transform.root.GetComponentsInChildren<Button>(true);

        foreach (var button in buttons)
        {
            button.onClick.RemoveListener(PlayButtonClickSFX);
            button.onClick.AddListener(PlayButtonClickSFX);
        }
    }

    private void PlayButtonClickSFX()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("Button_Click");
    }

    private void StartNewGame()
    {
        if (SaveManager.instance != null)
            SaveManager.instance.DeleteSaveData();

        GameManager.Instance.StartNewGame(newGameStartScene);
    }
}
