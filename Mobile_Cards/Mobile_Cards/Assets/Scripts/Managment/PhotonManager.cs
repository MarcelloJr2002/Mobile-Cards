using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitializePhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Already connected. Disconnecting first.");
            PhotonNetwork.Disconnect();
        }

        Debug.Log("Attempting to connect to Photon...");
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("ConnectUsingSettings called");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master in Photon.");
        PhotonNetwork.JoinLobby();
        //InitializeGameManager();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Photon Lobby.");
        SceneManager.LoadScene("CreateAndJoinRoom");
    }
}
