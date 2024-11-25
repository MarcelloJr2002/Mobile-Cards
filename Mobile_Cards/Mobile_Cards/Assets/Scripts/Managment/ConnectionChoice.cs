using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionChoice : MonoBehaviour
{
    public Text message;
    
    public void ChoosePhoton()
    {
        GameModeManager.Instance.selectedMode = GameModeManager.GameMode.Photon;
        GameModeManager.Instance.InitializeGameMode();
    }

    public void ChooseBluetooth()
    {
        GameModeManager.Instance.selectedMode = GameModeManager.GameMode.Bluetooth;
        //GameModeManager.Instance.InitializeGameMode();
        Debug.Log("BT");
        message.text = "Bluetooth";
        SceneManager.LoadScene("BTConnect");
    }
}
