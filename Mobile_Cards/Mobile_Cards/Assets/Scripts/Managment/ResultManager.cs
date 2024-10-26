using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public GameObject Result;
    public TMP_Text ResultText;

    public void DisplayWinResult()
    {
        Result.SetActive(true);
        ResultText.text = "  YOU WON!";
    }

    public void DisplayLoseResult()
    {
        Result.SetActive(true);
        ResultText.text = "  YOU LOST!";
    }

    public void DisplayDrawResult()
    {
        Result.SetActive(true);
        ResultText.text = "  DRAW!";
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
