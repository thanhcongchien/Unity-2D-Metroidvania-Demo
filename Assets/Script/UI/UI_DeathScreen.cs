using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
   public void GoToCampBTN()
    {
        GameManager.Instance.ChangeScene("Level_0", RespawnType.None);
    }
    public void GoToCheckPointBTN()
    {
        GameManager.Instance.RestartScene();
    }
    public void GoToMainMenu()
    {
        GameManager.Instance.ChangeScene("MainMenu", RespawnType.None);
    }
}
