using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBackButton : MonoBehaviourPunCallbacks
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

        if(sceneName == "MakaoRules")
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

    public void MakaoRulesScene()
    {
        SceneManager.LoadScene("MakaoRules");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene("GameChoice");
    }
}
