using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionChoice : MonoBehaviour
{
    public void ChoosePhoton()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.selectedMode = GameModeManager.GameMode.Photon;
            gameModeManager.InitializeGameMode();
            SceneManager.LoadScene("BlackJack");
        }
    }
}
