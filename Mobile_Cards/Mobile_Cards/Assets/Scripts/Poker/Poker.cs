using Photon.Pun;
using SVSBluetooth;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Poker : BaseGameManager
{
    public Transform[] playerPositions;
    public Button[] positionButtons;
    public CardDealer cardDealer;
    public Deck deck;
    public int currentPlayerIndex = 0;
    public Text[] betText;
    public Text[] MoneyText;
    public string playerId = "";
    private Dictionary<string, Player> playersList = new Dictionary<string, Player>();
    private PhotonView photonView;



    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        BluetoothForAndroid.ReceivedStringMessage += OnRecivedStringMessage;
    }

    public bool PhotonOrBluetooth()
    {
        if (GameModeManager.Instance.selectedMode == GameModeManager.GameMode.Photon)
        {
            return true;
        }
        else return false;
    }

    [PunRPC]
    public void AddPlayer(string playerName, Vector3 position)
    {
        Player newPlayer = new(PhotonNetwork.LocalPlayer);
        playersList.Add(playerName, newPlayer);
        playersList[playerName].position = position;
    }

    public void PositionButtonClicked(GameObject button)
    {
        if (PhotonOrBluetooth())
        {
            PhotonNetwork.LocalPlayer.NickName = "PhotonPlayer" + PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("AddPlayer", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.NickName, button.transform.position);

            photonView.RPC("HideButton", RpcTarget.AllViaServer, button.name);

            if (PhotonNetwork.LocalPlayer.IsLocal)
            {
                foreach (var positionBtn in positionButtons)
                {
                    positionBtn.gameObject.SetActive(false);
                }
            }
        }
    }

    public override void GameStart()
    {
        base.GameStart();
        if(playersList.Count >=2)
        {
            DealInitialCards();
        }
    }

    [PunRPC]
    public void DealCardToPlayer(string pId, int cardId)
    {
        Card card = deck.GetCardById(cardId);
        playersList[pId].AddCardToHand(card);
        cardDealer.DealCards(playersList[pId], cardId);
    }

    private void DealInitialCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < playersList.Count; j++)
                {
                    playerId = playersList.Keys.ElementAt(j);
                    Card drawnCard = deck.DrawCard();
                    Debug.Log(drawnCard.value);
                    photonView.RPC("DealCardToPlayer", RpcTarget.AllViaServer, playerId, drawnCard.id);
                }
            }
        }
    }

    public void Call()
    {

    }

    public void Raise()
    {

    }

    public void Fold()
    {

    }

    public void Result()
    {

    }

    void OnRecivedStringMessage(string message)
    {

    }
}
