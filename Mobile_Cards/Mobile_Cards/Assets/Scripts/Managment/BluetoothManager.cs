using SVSBluetooth;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameModeManager;

public class BluetoothManager : MonoBehaviour
{
    public static BluetoothManager Instance { get; private set; }

    private bool waitingForConnection = false;
    public Text messageText;
    public string masterClient = "";

    private Dictionary<GameType, string> gameScenes = new Dictionary<GameType, string>
    {
        { GameType.BlackJack, "BlackJack" },
        { GameType.Poker, "Poker" },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("BluetoothManager already exists. Destroying duplicate instance.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("BluetoothManager initialized.");
        }
    }

    private void OnEnable()
    {
        BluetoothForAndroid.ReceivedByteMessage += GetMessage;
        BluetoothForAndroid.DeviceConnected += OnConnectedToBluetooth;
        BluetoothForAndroid.DeviceSelected += OnDeviceSelected;
        BluetoothForAndroid.FailConnectToServer += OnFailedToConnect;
    }

    private void OnDisable()
    {
        BluetoothForAndroid.ReceivedByteMessage -= GetMessage;
        BluetoothForAndroid.DeviceConnected -= OnConnectedToBluetooth;
        BluetoothForAndroid.DeviceSelected -= OnDeviceSelected;
        BluetoothForAndroid.FailConnectToServer -= OnFailedToConnect;
    }

    public void InitializeBT()
    {
        BluetoothForAndroid.Initialize();
        messageText.text = "Initialized";
        Debug.Log("Initialized!");

        if (!BluetoothForAndroid.IsBTEnabled())
        {
            BluetoothForAndroid.EnableBT();
        }
    }

    private void OnConnectedToBluetooth()
    {
        Debug.Log("Connected to Bluetooth device.");
        if (waitingForConnection)
        {
            waitingForConnection = false;
            LoadGameScene();
        }
    }

    private void OnDeviceSelected(string device)
    {
        Debug.Log($"Device selected: {device}");
        waitingForConnection = true;
        LoadGameScene();
    }

    private void OnFailedToConnect()
    {
        Debug.Log("Failed to connect to Bluetooth device. Retrying...");
        waitingForConnection = false;
        BluetoothForAndroid.ConnectToServer("562a93dc-19d4-449e-b2b0-7deb5459c743");
    }

    private void GetMessage(byte[] message)
    {
        string receivedMessage = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log($"Message received: {receivedMessage}");
    }

    public void CreateBTServer()
    {
        if(messageText.text != "Initialized")
        {
            InitializeBT();
        }
        BluetoothForAndroid.CreateServer("562a93dc-19d4-449e-b2b0-7deb5459c743");
        Debug.Log("Bluetooth server created.");
        waitingForConnection = true;
        masterClient = Globals.localPlayerId;
        messageText.text = "Server created";
    }

    public void ConnectToBTServer()
    {
        if (messageText.text != "Initialized")
        {
            InitializeBT();
        }
        BluetoothForAndroid.ConnectToServer("562a93dc-19d4-449e-b2b0-7deb5459c743");
        Debug.Log("Starting to discover devices...");
    }

    private void LoadGameScene()
    {
        if (GameModeManager.Instance != null)
        {
            string sceneName = gameScenes[GameModeManager.Instance.selectedGameType];
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("GameModeManager instance not found. Cannot load scene.");
        }
    }


    public void BtnMessage()
    {
        Debug.Log("Button Clicked!");
    }
}
