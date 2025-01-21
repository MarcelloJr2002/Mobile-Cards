using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SVSBluetooth;

public class GameModeManager : MonoBehaviour
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

    public PhotonManager photonManager;

    public void InitializeGameMode()
    {
        switch (selectedMode)
        {
            case GameMode.Photon:
                photonManager.InitializePhoton();
                break;

            case GameMode.Bluetooth:
                BluetoothManager.Instance.InitializeBT();
                break;
            default:
                Debug.Log("Unknown game mode!");
                break;
        }
    }


    void InitializeGameManager()
    {
        switch(selectedGameType)
        {
            case GameType.BlackJack:
                gameManager = gameObject.AddComponent<BlackJackGame>();
                break;
            case GameType.Poker:
                gameManager = gameObject.AddComponent<Poker>();
                break;
            case GameType.Makao:
                gameManager = gameObject.AddComponent<Makao>();
                break;
        }

        gameManager.GameStart();
    }
}
