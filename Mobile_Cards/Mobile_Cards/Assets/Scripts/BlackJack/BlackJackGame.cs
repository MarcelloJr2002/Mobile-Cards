using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;

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
    private int index = 0;
    private new PhotonView photonView;

    private void Start()
    {
        dealerBot = new Player();
        //dealerBot.SetPosition(dealerPosition.position);
        photonView = GetComponent<PhotonView>();
    }
    

    /*public void AssignPositions(Player player)
    {
        Debug.Log("AssingPosition");
        Debug.Log(playerPositions[0].position);
        switch(players.Count)
        {
            case 1:
                player.position = playerPositions[0].position;
                break;
            case 2:
                player.position = playerPositions[1].position;
                break;
            case 3:
                player.position = playerPositions[2].position;
                break;
        }
        Debug.Log(player.position);
    }*/

    public void PositionButtonClicked(GameObject button)
    {

        Player newPlayer = new(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LocalPlayer.NickName = "PhotonPlayer" + index;
        playersList.Add(PhotonNetwork.LocalPlayer.NickName, newPlayer);
        playersList[PhotonNetwork.LocalPlayer.NickName] = newPlayer;
        index++;
        Debug.Log(playersList.Count);
        Debug.Log("Available Players: " + playersList.Count);
        Debug.Log("Local Player Name: " + PhotonNetwork.LocalPlayer.NickName);
        Debug.Log(button.transform.position);
        newPlayer.position = button.transform.position;
        Debug.Log(newPlayer.position);

        Debug.Log(button.name);
        photonView.RPC("HideButton", RpcTarget.All, button);

        foreach (var positionBtn in positionButtons)
        {
            positionBtn.gameObject.SetActive(false);
        }

    }

    [PunRPC]
    public void HideButton(GameObject buttonName)
    {
        Debug.Log(buttonName);
        //Debug.Log(GameObject.Find(buttonName));
        //GameObject newbutton = GameObject.Find(buttonName);

        /*if (newbutton != null)
        {
            newbutton.SetActive(false);
        }*/
    }

    /*
    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        

        // Pobierz gracza ze słownika i przypisz pozycję
        //Player playerToAssign = players[newPlayer.NickName];
        //AssignPositions(playerToAssign);
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
    public void UpdateMoneyText(double amount, int index)
    {
        MoneyText[index].text = amount.ToString() + "$";
    }

    [PunRPC]
    public void DisplayBet(double betValue, int index)
    {
        betText[index].text = betValue.ToString() + "$";
    }
    /*
    public void SetBet(double betValue)
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            playerId = players.Keys.ElementAt(currentPlayerIndex);

            Player currentPlayer = players[playerId];

            currentPlayer.bet += betValue;
            currentPlayer.DeductMoney(betValue);

            double playerMoney = currentPlayer.money;

            photonView.RPC("DisplayBet", RpcTarget.All, betValue, currentPlayerIndex);
            photonView.RPC("UpdateMoneyText", RpcTarget.All, playerMoney, currentPlayerIndex);
        }
    }

    public void BetTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Bet turn");

            playerId = players.Keys.ElementAt(currentPlayerIndex);

            ChipButtons.SetActive(playerId == photonView.Owner.UserId);
            ConfirmButton.SetActive(playerId == photonView.Owner.UserId);
        }
    }

    

    public void ConfirmButtonClicked()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            ChipButtons.SetActive(false);
            ConfirmButton.SetActive(false);
            currentPlayerIndex += 1;

            if (currentPlayerIndex >= players.Count)
            {
                EndBetingTurn();
            }

            photonView.RPC("UpdateBetTurn", RpcTarget.All, currentPlayerIndex);
        }
    }

    public void EndBetingTurn()
    {
        currentPlayerIndex = 0;
        DealInitialCards();
        HitStandTurn();
    }

    [PunRPC]
    public void UpdateBetTurn(int nextPlayerIndex)
    {
        currentPlayerIndex = nextPlayerIndex;
        BetTurn();
    }


    
    public override void GameStart()
    {
        base.GameStart();
        currentPlayerIndex = 0;
    }


    /*
    [PunRPC]
    public void DealCardsToPlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                playerId = players.Keys.ElementAt(j);
                StartCoroutine(cardDealer.DealCards(players[playerId]));
            }
        }
    }

    private void DealInitialCards()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("DealCardsToPlayers", RpcTarget.All);
        }
    }

    
    [PunRPC]
    public void PlayerHit(string playerIndex)
    {
        StartCoroutine(cardDealer.DealCards(players[playerIndex]));
        if (players[playerIndex].Busted())
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex);
            Stand();
        }
    }

    public void HitStandTurn()
    {
        HitStandBtns.SetActive(currentPlayerIndex == PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }


    public void Hit()
    {
        playerId = players.Keys.ElementAt(currentPlayerIndex);
        photonView.RPC("PlayerHit", RpcTarget.All, playerId);
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
        if(currentPlayerIndex < players.Count)
        {
            photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
        }

        else
        {
            currentPlayerIndex = 0;
            DealerTurn();
            Result();
        }
    }
 
    public void DealerTurn()
    {
        while (dealerBot.score < 17)
        {
            StartCoroutine(cardDealer.DealCards(dealerBot));
        }
        if (dealerBot.Busted())
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex);
        }
    }

    private IEnumerator ShowAndHideSign(GameObject sign, Transform position)
    {
        sign.transform.position = position.position;
        sign.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        sign.SetActive(false);
    }

    [PunRPC]
    public void ShowAndHideResultSign(int signIndex, Transform position)
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
            StartCoroutine(ShowAndHideSign(signToShow, position));
        }
    }

    

    public void Result()
    {
        double betMoney = 0;

        for (int i=0; i<players.Count; i++)
        {
            playerId = players.Keys.ElementAt(i);
            if (players[playerId].Busted())
            {
                i++;
            }

            if (players[playerId].score > dealerBot.score)
            {
                betMoney += players[playerId].bet * 2;
                players[playerId].AddMoney(betMoney);
                signIndex = 1;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
                photonView.RPC("UpdateMoneyText", RpcTarget.All, players[playerId].money, i);
            }

            if (players[playerId].score < dealerBot.score)
            {
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
            }

            if (players[playerId].score == dealerBot.score)
            {

                if (players[playerId].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    betMoney = players[playerId].bet + players[playerId].bet * 1.5;
                    players[playerId].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[playerId].money, i);
                }

                if (dealerBot.IfBlackJack() && !players[playerId].IfBlackJack())
                {
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
                }

                if (players[playerId].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    betMoney += players[playerId].bet;
                    players[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[playerId].money, i);
                }

                else
                {
                    betMoney += players[playerId].bet;
                    players[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[playerId].money, i);
                }
            }

        }

        EndGame();
    }

    
    public override void EndGame()
    {
        base.EndGame();
        for (int i=0; i<players.Count; i++)
        {
            playerId = players.Keys.ElementAt(i);
            players[playerId].ResetHand();
            players[playerId].score = 0;
            players[playerId].bet = 0;
        }
        dealerBot.ResetHand();
        dealerBot.score = 0;
        for(int i=0; i<betText.Length; i++)
        {
            betText[i].text = "";
        }

        GameStart();
    }*/


}
