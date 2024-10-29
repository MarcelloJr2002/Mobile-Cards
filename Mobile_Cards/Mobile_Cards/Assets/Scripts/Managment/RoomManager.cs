using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public InputField createInputField;
    public InputField joinInputField;

    public static RoomManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInputField.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInputField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("BlackJack");
    }
}
