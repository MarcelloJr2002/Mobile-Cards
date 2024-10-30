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
    private PhotonView photonView;

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

    [PunRPC]
    public void AddPlayer(string playerName, Vector3 position)
    {
        Player newPlayer = new(PhotonNetwork.LocalPlayer);
        playersList.Add(playerName, newPlayer);
        playersList[playerName].position = position;

        // Dodatkowe debugowanie
        Debug.Log($"Added player: {playerName} at position: {position}");
        Debug.Log(playersList.Count);
    }

    public void PositionButtonClicked(GameObject button)
    {
        PhotonNetwork.LocalPlayer.NickName = "PhotonPlayer" + PhotonNetwork.LocalPlayer.ActorNumber;

        photonView.RPC("AddPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, button.transform.position);

        if (PhotonOrBluetooth())
        {
            photonView.RPC("HideButton", RpcTarget.Others, button.name);
        }

        if(PhotonNetwork.LocalPlayer.IsLocal)
        {
            foreach (var positionBtn in positionButtons)
            {
                positionBtn.gameObject.SetActive(false);
            }
        }

        if (playersList.Count >= 2)
        {
            GameStart();
        }

    }

    [PunRPC]
    public void HideButton(string buttonName)
    {
        Debug.Log(buttonName);
        GameObject newbutton = GameObject.Find(buttonName);

        if (newbutton != null && newbutton.activeSelf)
        {
            newbutton.SetActive(false);
        }
    }



    
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(playersList.Count);
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
    
    public void SetBet(double betValue)
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < playersList.Count)
        {
            playerId = playersList.Keys.ElementAt(currentPlayerIndex);
            playersList[playerId].bet += betValue;
            playersList[playerId].DeductMoney(betValue);


            double playerMoney = playersList[playerId].money;

            photonView.RPC("DisplayBet", RpcTarget.AllViaServer, betValue, currentPlayerIndex);
            photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playerMoney, currentPlayerIndex);
        }
    }
    
    public void BetTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Bet turn");

            playerId = playersList.Keys.ElementAt(currentPlayerIndex);

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

            if (currentPlayerIndex >= playersList.Count)
            {
                EndBetingTurn();
            }

            photonView.RPC("UpdateBetTurn", RpcTarget.AllViaServer, currentPlayerIndex);
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
        BetTurn();
    }


    
    [PunRPC]
    public void DealCardsToPlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < playersList.Count; j++)
            {
                playerId = playersList.Keys.ElementAt(j);
                StartCoroutine(cardDealer.DealCards(playersList[playerId]));
            }
        }
    }

    private void DealInitialCards()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("DealCardsToPlayers", RpcTarget.AllViaServer);
        }
    }

    
    [PunRPC]
    public void PlayerHit(string playerIndex)
    {
        StartCoroutine(cardDealer.DealCards(playersList[playerIndex]));
        if (playersList[playerIndex].Busted())
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex);
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
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex);
        }
    }

    private IEnumerator ShowAndHideSign(GameObject sign, Vector3 position)
    {
        sign.transform.position = position;
        sign.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        sign.SetActive(false);
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
            StartCoroutine(ShowAndHideSign(signToShow, position));
        }
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
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
                photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, i);
            }

            if (playersList[playerId].score < dealerBot.score)
            {
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
            }

            if (playersList[playerId].score == dealerBot.score)
            {

                if (playersList[playerId].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    betMoney = playersList[playerId].bet + playersList[playerId].bet * 1.5;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, i);
                }

                if (dealerBot.IfBlackJack() && !playersList[playerId].IfBlackJack())
                {
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
                }

                if (playersList[playerId].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, i);
                }

                else
                {
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, i);
                }
            }

        }

        EndGame();
    }

    
    public override void EndGame()
    {
        base.EndGame();
        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            playersList[playerId].ResetHand();
            playersList[playerId].score = 0;
            playersList[playerId].bet = 0;
        }
        dealerBot.ResetHand();
        dealerBot.score = 0;
        for(int i=0; i<betText.Length; i++)
        {
            betText[i].text = "";
        }

        GameStart();
    }


}
