using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class BlackJackGame : BaseGameManager
{
    public Transform[] playerPositions;
    public Transform dealerPosition;
    public GameObject playerPrefab;
    public Player[] players;
    public Player dealerBot;

    private int currentPlayers = 0;
    //private bool gameStarted = false;
    private int maxPlayers = 3;
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

    private void Start()
    {
        currentPlayers = PhotonNetwork.PlayerList.Length;
        players = new Player[maxPlayers];

        dealerBot = gameObject.AddComponent<Player>();
        dealerBot.position.position = dealerPosition.position;

        TwentyBet.onClick.AddListener(() => SetBet(betValues[0]));
        FiftyBet.onClick.AddListener(() => SetBet(betValues[1]));
        HundredBet.onClick.AddListener(() => SetBet(betValues[2]));
        TwoHundredBet.onClick.AddListener(() => SetBet(betValues[3]));

        for(int i=0; i<betText.Length; i++)
        {
            betText[i].text = "";
            MoneyText[i].text = "";
        }

        if (currentPlayers <= maxPlayers && currentPlayers > 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Przypisanie pozycji dla Master Clienta (gracz 0)
                AssignPositions(PhotonNetwork.LocalPlayer, 0);
            }

            // Sprawdź, czy są inni gracze w pokoju
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (!PhotonNetwork.PlayerList[i].IsMasterClient)
                {
                    // Przypisz pozycję dla innych graczy
                    AssignPositions(PhotonNetwork.PlayerList[i], i);
                }
            }

            GameStart();
        }

    }


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
        players[currentPlayerIndex].bet += betValue;
        players[currentPlayerIndex].DeductMoney(betValue);
        double playerMoney = players[currentPlayerIndex].money;
        photonView.RPC("DisplayBet", RpcTarget.All, betValue, currentPlayerIndex);
        photonView.RPC("UpdateMoneyText", RpcTarget.All, playerMoney, currentPlayerIndex);
    }

    private void AssignPositions(Photon.Realtime.Player player, int index)
    {
        Vector3 spawnPosition = playerPositions[index].position;

        if(player.IsLocal)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
            players[index] = gameObject.AddComponent<Player>();
            players[index].position.position = spawnPosition;
            photonView.RPC("UpdateMoneyText", RpcTarget.All, players[index].money, index);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Sprawdzamy, ilu graczy jest obecnie w pokoju
        currentPlayers = PhotonNetwork.PlayerList.Length;

        // Przydzielamy pozycję dla nowego gracza
        AssignPositions(newPlayer, currentPlayers - 1);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        currentPlayers = PhotonNetwork.PlayerList.Length;

        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player playerScript = playerObj.GetComponent<Player>();

            if (playerScript.photonPlayer != null && playerScript.photonPlayer.ActorNumber == otherPlayer.ActorNumber)
            {
                Destroy(playerObj);
                break;
            }
        }

    }


    public void BetTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentPlayerIndex == photonView.Owner.ActorNumber - 1)
            {
                ChipButtons.SetActive(true);
                ConfirmButton.SetActive(true);
            }

            else
            {
                ChipButtons.SetActive(false);
                ConfirmButton.SetActive(false);
            }
        }
    }

    public void ConfirmButtonClicked()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            ChipButtons.SetActive(false);
            ConfirmButton.SetActive(false);
            currentPlayerIndex += 1;

            if(currentPlayerIndex >= players.Length)
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
        BetTurn();
    }



    [PunRPC]
    public void DealCardsToPlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                StartCoroutine(cardDealer.DealCards(players[j]));
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
    public void PlayerHit(int playerIndex)
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
        photonView.RPC("PlayerHit", RpcTarget.All, currentPlayerIndex);
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
        if(currentPlayerIndex < players.Length)
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
            StartCoroutine(dealer.DealCards(dealerBot));
        }
        if (dealerBot.Busted())
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex);
        }
    }

    private IEnumerator ShowAndHideBustedSign(Transform position)
    {
        bustedSign.transform.position = position.position;
        bustedSign.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        bustedSign.SetActive(false);
    }

    private IEnumerator ShowAndHideWinnerSign(Transform position)
    {
        winnerSign.transform.position = position.position;
        winnerSign.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        winnerSign.SetActive(false);
    }

    private IEnumerator ShowAndHideLoseSign(Transform position)
    {
        loseSign.transform.position = position.position;
        loseSign.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        loseSign.SetActive(false);
    }

    private IEnumerator ShowAndHideDrawSign(Transform position)
    {
        drawSign.transform.position = position.position;
        drawSign.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        drawSign.SetActive(false);
    }


    [PunRPC]
    public void ShowAndHideResultSign(int signIndex, Transform position)
    {
        switch (signIndex) 
        {
            case 0:
                StartCoroutine(ShowAndHideBustedSign(position));
                break;
            case 1:
                StartCoroutine(ShowAndHideWinnerSign(position));
                break;
            case 2:
                StartCoroutine(ShowAndHideLoseSign(position));
                break;
            case 3:
                StartCoroutine(ShowAndHideDrawSign(position));
                break;
        }
    }


    public void Result()
    {
        double betMoney = 0;
        
        for (int i=0; i<players.Length; i++)
        {
            if (players[i].Busted())
            {
                i++;
            }

            if (players[i].score > dealerBot.score)
            {
                betMoney += players[i].bet * 2;
                players[i].AddMoney(betMoney);
                signIndex = 1;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
                photonView.RPC("UpdateMoneyText", RpcTarget.All, players[i].money, i);
            }

            if (players[i].score < dealerBot.score)
            {
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
            }

            if (players[i].score == dealerBot.score)
            {

                if (players[i].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    betMoney = players[i].bet + players[i].bet * 1.5;
                    players[i].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[i].money, i);
                }

                if (dealerBot.IfBlackJack() && !players[i].IfBlackJack())
                {
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
                }

                if (players[i].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    betMoney += players[i].bet;
                    players[i].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[i].money, i);
                }

                else
                {
                    betMoney += players[i].bet;
                    players[i].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.All, signIndex, players[i].position.position);
                    photonView.RPC("UpdateMoneyText", RpcTarget.All, players[i].money, i);
                }
            }

        }

        EndGame();
    }

    public override void EndGame()
    {
        base.EndGame();
        for(int i=0; i<players.Length; i++)
        {
            players[i].ResetHand();
            players[i].score = 0;
            players[i].bet = 0;
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
