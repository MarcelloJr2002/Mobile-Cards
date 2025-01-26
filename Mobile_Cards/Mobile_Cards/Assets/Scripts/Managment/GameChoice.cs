using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameChoice : MonoBehaviour
{
    public void ChooseBlackJack()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.selectedGameType = GameModeManager.GameType.BlackJack;
            ChoosePhoton();
            //SceneManager.LoadScene("ConnectionChoice");
        }
    }

    public void ChoosePoker()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.selectedGameType = GameModeManager.GameType.Poker;
            ChoosePhoton();
            //SceneManager.LoadScene("ConnectionChoice");
        }
    }

    public void ChooseMakao()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.selectedGameType = GameModeManager.GameType.Makao;
            ChoosePhoton();
            //SceneManager.LoadScene("ConnectionChoice");
        }
    }

    public void ChoosePhoton()
    {
        SceneManager.LoadScene("LoadingScene");
        GameModeManager.Instance.selectedMode = GameModeManager.GameMode.Photon;
        GameModeManager.Instance.InitializeGameMode();
    }
}
