using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SVSBluetooth;

public class GameModeManager : MonoBehaviourPunCallbacks
{
    public static GameModeManager Instance { get; private set; }

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

    public enum GameMode
    {
        Photon,
        Bluetooth
    }

    public enum GameType
    {
        BlackJack,
        Poker,
        Makao
    }

    public GameMode selectedMode;
    public GameType selectedGameType;
    public BaseGameManager gameManager;

    /*private void OnEnable()
    {
        BluetoothForAndroid.ReceivedByteMessage += GetMessage;
        BluetoothForAndroid.DeviceConnected += OnConnectedToBluetooth;
        BluetoothForAndroid.FailConnectToServer += OnFailedToConnect;
    }

    private void OnDisable()
    {
        BluetoothForAndroid.ReceivedByteMessage -= GetMessage;
        BluetoothForAndroid.DeviceConnected -= OnConnectedToBluetooth;
        BluetoothForAndroid.FailConnectToServer -= OnFailedToConnect;
    }*/

    void OnConnectedToBluetooth()
    {
        Debug.Log("Connected to Bluetooth device.");
    }

    void OnFailedToConnect()
    {
        Debug.Log("Failed to connect to Bluetooth device. Retrying...");
    }

    void GetMessage(byte[] message)
    {
        Debug.Log("Message received: " + System.Text.Encoding.UTF8.GetString(message));
    }

    public void InitializeGameMode()
    {
        switch (selectedMode)
        {
            case GameMode.Photon:
                InitializePhoton();
                break;

            case GameMode.Bluetooth:
                InitializeBluetooth();
                break;
            default:
                Debug.Log("Unknown game mode!");
                break;
        }
    }

    void InitializePhoton()
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



    void InitializeBluetooth()
    {
        BluetoothForAndroid.Initialize();

        if (!BluetoothForAndroid.IsBTEnabled())
        {
            BluetoothForAndroid.EnableBT();
        }

        BluetoothForAndroid.CreateServer("562a93dc-19d4-449e-b2b0-7deb5459c743");
    }


    void InitializeGameManager()
    {
        switch(selectedGameType)
        {
            case GameType.BlackJack:
                gameManager = gameObject.AddComponent<BlackJackGame>();
                break;
            case GameType.Poker:
                gameManager = gameObject.AddComponent<PokerGameManager>();
                break;
            case GameType.Makao:
                gameManager = gameObject.AddComponent<MakaoGameManager>();
                break;
        }

        gameManager.GameStart();
    }


}
