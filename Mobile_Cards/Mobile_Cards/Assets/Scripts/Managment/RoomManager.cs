using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public InputField createInputField;
    public InputField joinInputField;
    public Text infoText;


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

    public void ShowText(string text)
    {
        // Ustaw początkową przezroczystość na 0 (tekst niewidoczny)
        infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, 0);
        infoText.text = text;

        // Animacja pojawienia się
        infoText.DOFade(1, 0.5f) // Fade-in w 0.5 sekundy
            .OnComplete(() =>
            {
                // Po 2 sekundach zniknij
                infoText.DOFade(0, 0.5f).SetDelay(2f); // Fade-out w 0.5 sekundy po opóźnieniu 2 sekund
            });
    }

    public bool ContainsPolishCharacters(string input)
    {
        // Wyrażenie regularne sprawdzające polskie znaki
        string polishCharsPattern = "[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]";
        return Regex.IsMatch(input, polishCharsPattern);
    }

    public void CreateRoom()
    {
        if(createInputField.text == "")
        {
            infoText.text = "Input fiedl cannot be empty!";
            ShowText(infoText.text);
            Debug.Log("Input fiedl cannot be empty!");
        }

        if (ContainsPolishCharacters(createInputField.text))
        {
            infoText.text = "You can't use polish letters!";
            ShowText(infoText.text);
        }

        else
        {
            PhotonNetwork.CreateRoom(createInputField.text);
        }
    }

    public void JoinRoom()
    {
        if (joinInputField.text == "")
        {
            infoText.text = "Input fiedl cannot be empty!";
            ShowText(infoText.text);
            Debug.Log("Input fiedl cannot be empty!");
        }

        if (ContainsPolishCharacters(joinInputField.text))
        {
            infoText.text = "You can't use polish letters!";
            ShowText(infoText.text);
        }

        else
        {
            PhotonNetwork.JoinRoom(joinInputField.text);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        infoText.text = "Room doesn't exist!";
        ShowText(infoText.text);
    }

    public override void OnJoinedRoom()
    {
        if(GameModeManager.Instance.selectedGameType == GameModeManager.GameType.BlackJack)
        {
            PhotonNetwork.LoadLevel("BlackJack");
        }

        else if(GameModeManager.Instance.selectedGameType == GameModeManager.GameType.Poker)
        {
            PhotonNetwork.LoadLevel("Poker");
        }

        else if(GameModeManager.Instance.selectedGameType == GameModeManager.GameType.Makao)
        {
            PhotonNetwork.LoadLevel("Makao");
        }
    }
}
