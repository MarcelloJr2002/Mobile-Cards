using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;

public class BlackJackGame : BaseGameManager
{
    public Transform[] playerPositions;
    public Transform dealerPosition;
    //public GameObject playerPrefab;
    //public Player[] players;
    public Player dealerBot;
    public Button[] positionButtons;

    //private int currentPlayers = 0;
    //private bool gameStarted = false;
    public CardDealer cardDealer;
    public int currentPlayerIndex = 0;
    public GameObject bustedSign;
    public GameObject winnerSign;
    public GameObject loseSign;
    public GameObject drawSign;
    public double[] betValues = { 20, 50, 100, 200 };
    public Button TwentyBet;
    public Button FiftyBet;
    public Button HundredBet;
    public Button TwoHundredBet;
    public Button StandButton;
    public GameObject HitStandBtns;
    public GameObject ChipButtons;
    public GameObject ConfirmButton;
    public Text[] betText;
    public int signIndex = 0;
    public Text[] MoneyText;
    public string playerId = "";
    private Dictionary<string, Player> playersList = new Dictionary<string, Player>();
    private PhotonView photonView;
    private int playersConfirmed = 0;
    public Deck deck;
    private Vector3 signPosition = new Vector3(0, 150, 0);

    private void Start()
    {
        dealerBot = new Player();
        //dealerBot.SetPosition(dealerPosition.position);
        photonView = GetComponent<PhotonView>();
    }

    public bool PhotonOrBluetooth()
    {
        if (GameModeManager.Instance.selectedMode == GameModeManager.GameMode.Photon)
        {
            return true;
        }
        else return false;
    }

    public void ChipButtonsAction()
    {
        TwentyBet.onClick.AddListener(() => SetBet(betValues[0]));
        FiftyBet.onClick.AddListener(() => SetBet(betValues[1]));
        HundredBet.onClick.AddListener(() => SetBet(betValues[2]));
        TwoHundredBet.onClick.AddListener(() => SetBet(betValues[3]));
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

    [PunRPC]
    public void HideButton(string buttonName)
    {
        GameObject newbutton = GameObject.Find(buttonName);

        if (newbutton != null && newbutton.activeSelf)
        {
            newbutton.SetActive(false);
        }

        if (playersList.Count >= 1)
        {
            GameStart();
        }
    }



    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        //Debug.Log(playersList.Count);
    }
    /*
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        //currentPlayers = PhotonNetwork.PlayerList.Length;

        /*foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player playerScript = playerObj.GetComponent<Player>();

            if (playerScript.photonPlayer != null && playerScript.photonPlayer.ActorNumber == otherPlayer.ActorNumber)
            {
                Destroy(playerObj);
                break;
            }
        }

    }*/


    [PunRPC]
    public void UpdateMoneyText(double amount, float x)
    {
        for(int i=0; i<MoneyText.Length; i++)
        {
            if (MoneyText[i].transform.position.x == x)
            {
                MoneyText[i].text = amount.ToString() + "$";
                break;
            }
        }
    }

    [PunRPC]
    public void DisplayBet(double betValue, float x)
    {
        for(int i=0; i<betText.Length; i++)
        {
            if (betText[i].transform.position.x == x)
            {
                betText[i].text = betValue.ToString() + "$";
                break;
            }
        }
    }

    public void SetBet(double betValue)
    {
        string localPlayerId = PhotonNetwork.LocalPlayer.NickName;

        if (playersList.ContainsKey(localPlayerId))
        {
            playersList[localPlayerId].bet += betValue;
            playersList[localPlayerId].DeductMoney(betValue);

            double playerMoney = playersList[localPlayerId].money;
            double playerBet = playersList[localPlayerId].bet;
            float xPosition = playersList[localPlayerId].position.x;

            photonView.RPC("DisplayBet", RpcTarget.All, playerBet, xPosition);
            photonView.RPC("UpdateMoneyText", RpcTarget.All, playerMoney, xPosition);
        }
    }


    public void BetTurn()
    {
        ChipButtons.SetActive(true);
        ConfirmButton.SetActive(true);
    }



    public void ConfirmButtonClicked()
    {
        ChipButtons.SetActive(false);
        ConfirmButton.SetActive(false);
        photonView.RPC("PlayerConfirmedBet", RpcTarget.All);
    }

    [PunRPC]
    public void PlayerConfirmedBet()
    {
        playersConfirmed++;

        if (playersConfirmed >= playersList.Count)
        {
            EndBetingTurn();
        }
    }

    public void EndBetingTurn()
    {
        currentPlayerIndex = 0;
        DealInitialCards();
        HitStandTurn();
    }


    
    public override void GameStart()
    {
        base.GameStart();
        currentPlayerIndex = 0;
        dealerBot.position = dealerPosition.position;
        ChipButtonsAction();
        BetTurn();
    }


    [PunRPC]
    public void DealCardToPlayer(string pId, int cardId)
    {
        Card card = deck.GetCardById(cardId);
        playersList[pId].AddCardToHand(card);
        cardDealer.DealCards(playersList[pId], cardId);
    }

    [PunRPC]
    public void DealCardToDealer(int id)
    {
        Card card = deck.GetCardById(id);
        dealerBot.AddCardToHand(card);
        cardDealer.DealCards(dealerBot, id);
    }


    private void DealInitialCards()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < playersList.Count; j++)
                {
                    playerId = playersList.Keys.ElementAt(j);
                    Card drawnCard = deck.DrawCard();
                    Debug.Log(drawnCard.value);
                    //playersList[playerId].AddCardToHand(drawnCard);
                    //cardDealer.DealCards(playersList[playerId], drawnCard.id);
                    photonView.RPC("DealCardToPlayer", RpcTarget.AllViaServer, playerId, drawnCard.id);
                }
                Card dealerCard = deck.DrawCard();
                Debug.Log(dealerCard.value);
                photonView.RPC("DealCardToDealer", RpcTarget.AllViaServer, dealerCard.id);
            }
        }
    }

    
    [PunRPC]
    public void PlayerHit(string playerIndex)
    {
        Card drawnCard = deck.DrawCard();
        playersList[playerIndex].AddCardToHand(drawnCard);
        photonView.RPC("DealCardToPlayer", RpcTarget.AllViaServer, playerId, drawnCard.id);
        Debug.Log(playersList[playerIndex].score);
        if (playersList[playerIndex].Busted())
        {
            Debug.Log("Busted");
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
            Stand();
        }
    }

    public void HitStandTurn()
    {
        HitStandBtns.SetActive(currentPlayerIndex == PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }


    public void Hit()
    {
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log(playerId);
        photonView.RPC("PlayerHit", RpcTarget.AllViaServer, playerId);
    }

    [PunRPC]
    public void UpdateTurn(int nextIndex)
    {
        currentPlayerIndex = nextIndex;
        HitStandTurn();
    }

    
    public void Stand()
    {
        currentPlayerIndex += 1;
        if(currentPlayerIndex < playersList.Count)
        {
            photonView.RPC("UpdateTurn", RpcTarget.AllViaServer, currentPlayerIndex);
        }

        else
        {
            currentPlayerIndex = 0;
            HitStandBtns.SetActive(false);
            DealerTurn();
        }
    }
 
    public void DealerTurn()
    {
        Card drawnCard;
        while (dealerBot.score < 17)
        {
            drawnCard = deck.DrawCard();
            dealerBot.AddCardToHand(drawnCard);
            //cardDealer.DealCards(dealerBot, drawnCard.id);
            photonView.RPC("DealCardToDealer", RpcTarget.AllViaServer, drawnCard.id);
        }
        Debug.Log("Dealer turn");
        if (dealerBot.Busted())
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, dealerBot.position + signPosition);
            Result();
        }

        else
        {
            Result();
        }
    }

    [PunRPC]
    public void ShowAndHideResultSign(int signIndex, Vector3 position)
    {
        GameObject signToShow = signIndex switch
        {
            0 => bustedSign,
            1 => winnerSign,
            2 => loseSign,
            3 => drawSign,
            _ => null
        };

        if (signToShow != null)
        {
            ShowAndHideSign(signToShow, position);
        }
    }

    //RPC
    private void ShowAndHideSign(GameObject sign, Vector3 position)
    {
        // Ustawiamy pozycję i aktywujemy znak
        sign.transform.position = position;
        sign.transform.SetAsLastSibling();
        sign.SetActive(true);

        // Resetujemy skalę do początkowej wartości
        sign.GetComponent<CanvasGroup>().alpha = 0; // Upewniamy się, że jest niewidoczne (wymaga CanvasGroup)

        // Animacja pojawiania się
        sign.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // Powiększenie z efektem
        sign.GetComponent<CanvasGroup>().DOFade(1, 0.5f); // Pojawianie się

        // Animacja zanikania po 2 sekundach
        DOVirtual.DelayedCall(3.0f, () =>
        {
            sign.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack); // Zmniejszenie
            sign.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                sign.SetActive(false); // Dezaktywacja znaku po zakończeniu animacji
            });
        });
    }



    public void Result()
    {
        double betMoney = 0;

        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            if (playersList[playerId].Busted())
            {
                i++;
            }

            if (playersList[playerId].score > dealerBot.score)
            {
                betMoney += playersList[playerId].bet * 2;
                playersList[playerId].AddMoney(betMoney);
                signIndex = 1;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);
            }

            if (playersList[playerId].score < dealerBot.score && !dealerBot.Busted())
            {
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
            }

            if (playersList[playerId].score == dealerBot.score && !playersList[playerId].Busted() && !dealerBot.Busted())
            {

                if (playersList[playerId].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    betMoney = playersList[playerId].bet + playersList[playerId].bet * 1.5;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);
                }

                if (dealerBot.IfBlackJack() && !playersList[playerId].IfBlackJack())
                {
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                }

                if (playersList[playerId].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);
                }

                else
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);
                }
            }

        }

        EndGame();
    }

    
    public override void EndGame()
    {
        base.EndGame();
        Debug.Log("EndGame");
        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            playersList[playerId].ResetHand();
            playersList[playerId].score = 0;
            playersList[playerId].bet = 0;
        }
        dealerBot.ResetHand();
        dealerBot.score = 0;
        for(int i=0; i<playersList.Count; i++)
        {
            DisplayBet(0, betText[i].transform.position.x);
        }

        //GameStart();
    }


}
