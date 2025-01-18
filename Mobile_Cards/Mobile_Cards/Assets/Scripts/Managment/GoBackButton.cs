using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBackButton : MonoBehaviour
{
    string sceneName;
    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    public void GoBack()
    {
        if (sceneName == "GameChoice")
        {
            SceneManager.LoadScene("MainMenu");
        }

        if (sceneName == "ConnectionChoice")
        {
            SceneManager.LoadScene("GameChoice");
        }

        if (sceneName == "PlayerManager")
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Already connected. Disconnecting first.");
                PhotonNetwork.Disconnect();
            }

            SceneManager.LoadScene("ConnectionChoice");
        }

        if (sceneName == "BTConnect")
        {
            SceneManager.LoadScene("PlayerManager");
        }

        if (sceneName == "CreateAndJoinRoom")
        {
            SceneManager.LoadScene("PlayerManager");
        }

        if (sceneName == "Rules")
        {
            SceneManager.LoadScene("MainMenu");
        }

        if (sceneName == "BlackJackRules")
        {
            SceneManager.LoadScene("Rules");
        }

        if (sceneName == "PokerRules")
        {
            SceneManager.LoadScene("Rules");
        }
    }

    public void BlackJackRulesScene()
    {
        SceneManager.LoadScene("BlackJackRules");
    }

    public void PokerRulesScene()
    {
        SceneManager.LoadScene("PokerRules");
    }
}
