using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void GameChoice()
    {
        SceneManager.LoadScene("GameChoice");
    }

    public void OptionsScene()
    {
        SceneManager.LoadScene("Options");
    }

    public void RulesScene()
    {
        SceneManager.LoadScene("Rules");
    }

    public void QuitGame()
    {
        Debug.Log("Wylaczanie gry...");
        Application.Quit();
    }
}
    
