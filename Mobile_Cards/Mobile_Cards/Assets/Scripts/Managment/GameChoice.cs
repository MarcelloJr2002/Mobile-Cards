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
            SceneManager.LoadScene("ConnectionChoice");
        }
    }

    public void ChoosePoker()
    {
        GameModeManager gameModeManager = FindObjectOfType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.selectedGameType = GameModeManager.GameType.Poker;
            SceneManager.LoadScene("ConnectionChoice");
        }
    }
}
