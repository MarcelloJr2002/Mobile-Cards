using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon.");
        InitializeGameManager();
    }

    void InitializeBluetooth()
    {
        //Do zrobienia
    }


    void OnConnectedToBluetooth()
    {
        //Do zrobienia
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
