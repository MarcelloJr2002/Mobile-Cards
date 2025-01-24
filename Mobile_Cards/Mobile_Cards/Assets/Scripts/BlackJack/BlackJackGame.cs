using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using SVSBluetooth;
using System;
using static SVSBluetooth.BluetoothForAndroid;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SceneManagement;
using System.Diagnostics.Contracts;

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
    public GameObject masterButton;
    public Text[] betText;
    public int signIndex = 0;
    public Text[] MoneyText;
    public Text[] pointsText;
    public string playerId = "";
    private Dictionary<string, Player> playersList = new Dictionary<string, Player>();
    private PhotonView photonView;
    private int playersConfirmed = 0;
    public Deck deck;
    public Deck rpcDeck;
    private Vector3 signPosition = new Vector3(0, 150, 0);
    private string message;
    public Text btText;
    public Text idText;
    private int score = 0;
    private int[] playersScore = new int[2];
    private bool showDealerCard = true;
    private int dealerCardsCount;
    private int[] playerCardsCount = new int[2];
    private int methodCount = 0;
    public Button testButton;
    public GameObject testObject;
    public GameObject cardPrefab;
    public Transform canvasTransform;
    public CardsModels cardsModels;
    public Sprite cardBackSprite;
    private void Start()
    {
        dealerBot = new Player();
        //dealerBot.SetPosition(dealerPosition.position);
        photonView = GetComponent<PhotonView>();

        if (GameModeManager.Instance.selectedMode == GameModeManager.GameMode.Bluetooth)
        {
            BluetoothForAndroid.ReceivedStringMessage += OnRecivedStringMessage;
        }

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
        methodCount++;
    }

    [PunRPC]
    public void AddPlayer(string playerName, Vector3 position)
    {
        Player newPlayer = new(PhotonNetwork.LocalPlayer);

        if(playersList.ContainsKey(playerName))
        {
            playerName += " 1";
        }
        playersList.Add(playerName, newPlayer);
        playersList[playerName].position = position;
    }

    public void PositionButtonClicked(GameObject button)
    {

        if (PhotonOrBluetooth())
        {
            //PhotonNetwork.LocalPlayer.NickName = "PhotonPlayer" + PhotonNetwork.LocalPlayer.ActorNumber;

            PhotonNetwork.LocalPlayer.NickName = Globals.localPlayerId;

            photonView.RPC("AddPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, button.transform.position);

            photonView.RPC("HideButton", RpcTarget.AllViaServer, button.name);

            if (PhotonNetwork.LocalPlayer.IsLocal)
            {
                foreach (var positionBtn in positionButtons)
                {
                    positionBtn.gameObject.SetActive(false);
                }
            }

            if(PhotonNetwork.IsMasterClient)
            {
                masterButton.SetActive(true);
            }
        }

        else
        {
            Player player = new(Globals.localPlayerId);
            playersList.Add(player.UserName, player);
            playersList[player.UserName].position = button.transform.position;


            message = $"ADDPLAYER,{button.transform.position.x},{button.transform.position.y},{button.transform.position.z},{player.UserName},{currentPlayerIndex}";
            BluetoothForAndroid.WriteMessage(message);
            string hideMessage = $"HIDEBUTTON,{button.name}";


            BluetoothForAndroid.WriteMessage(hideMessage);
            foreach (var positionBtn in positionButtons)
            {
                positionBtn.gameObject.SetActive(false);
            }

            if(playersList.Count == 1)
            {
                masterButton.SetActive(true);
            }
            currentPlayerIndex += 1;

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
    }




    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        //Debug.Log(playersList.Count);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player photonPlayer)
    {
        Debug.Log($"Gracz {photonPlayer.NickName} opuscil pokoj.");
        /*
        if (playersList.ContainsKey(photonPlayer.NickName))
        {
            photonView.RPC("RemovePlayer", RpcTarget.All, photonPlayer.NickName);
        }
        else
        {
            Debug.LogWarning($"Gracz {photonPlayer.NickName} nie został znaleziony na liście graczy.");
        }*/

        photonView.RPC("UpdateGameStateRPC", RpcTarget.AllBuffered);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("MainMenu");
    }

    [PunRPC]
    public void UpdateGameStateRPC()
    {
        UpdateGameState();
    }

    public void UpdateGameState()
    {
        if (PhotonOrBluetooth() && playersList.Count < 2)
        {
            photonView.RPC("EndGameRPC", RpcTarget.AllBuffered);

            masterButton.SetActive(true);
        }

        if(!PhotonOrBluetooth() && playersList.Count < 2)
        {
            masterButton.SetActive(true);
            message = $"ENDGAME";
            BluetoothForAndroid.WriteMessage(message);
        }
    }


    [PunRPC]
    public void RemovePlayer(string nickName)
    {
        if (playersList.ContainsKey(nickName))
        {
            playersList[nickName].ResetHand();
            playersList[nickName].score = 0;
            playersList[nickName].bet = 0;

            foreach (GameObject card in playersList[nickName].cardsObjects)
            {
                StartCoroutine(cardDealer.DestroyCards(card));
            }

            playersList.Remove(nickName);
            Debug.Log($"Gracz {nickName} zostal usuniety z gry.");
            SceneManager.LoadScene("CreateAndJoinRoom");
        }
        else
        {
            Debug.LogWarning($"Gracz {nickName} nie został znaleziony na liście graczy.");
        }
    }



    public void ReturnToLobby()
    {
        if (playersList.ContainsKey(PhotonNetwork.LocalPlayer.NickName))
        {
            photonView.RPC("RemovePlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        }

        PhotonNetwork.LeaveRoom();
    }




    [PunRPC]
    public void UpdateMoneyText(double amount, float x)
    {
        UpdateMoney(amount, x);
    }

    [PunRPC]
    public void DisplayBet(double betValue, float x)
    {
        Debug.Log("Bet rpc: " + betValue);
        ShowBet(betValue, x);
    }

    public void UpdateMoney(double amount, float x)
    {
        for (int i = 0; i < MoneyText.Length; i++)
        {
            if (MoneyText[i].transform.position.x == x)
            {
                Debug.Log("UpdateMoney value: " + amount);
                MoneyText[i].text = amount.ToString() + "$";
                Debug.Log(MoneyText[i].text);
                if(!PhotonOrBluetooth())
                {
                    message = $"UPDATEMONEY,{MoneyText[i].text},{i}";
                    BluetoothForAndroid.WriteMessage(message);
                }
                break;
            }
        }
    }

    public void ShowBet(double betValue, float x)
    {
        for (int i = 0; i < betText.Length; i++)
        {
            if (betText[i].transform.position.x == x)
            {
                Debug.Log("ShowBet value: " + betValue);
                betText[i].text = betValue.ToString() + "$";
                Debug.Log(betText[i].text);
                if (!PhotonOrBluetooth())
                {
                    message = $"SHOWBET,{betText[i].text},{i}";
                    BluetoothForAndroid.WriteMessage(message);
                }
                break;
            }
        }
    }

    public void SetBet(double betValue)
    {
        string localPlayerId;
        if (PhotonOrBluetooth())
        {
            localPlayerId = PhotonNetwork.LocalPlayer.NickName;

            SetBetCheckPlayer(localPlayerId, betValue);
        }

        else
        {
            localPlayerId = Globals.localPlayerId;

            SetBetCheckPlayer(localPlayerId, betValue);
        }
    }

    public void SetBetCheckPlayer(string localPlayerId, double betValue)
    {
        if (playersList.ContainsKey(localPlayerId))
        {
            methodCount++;
            Debug.Log("Method count: " + methodCount);
            playersList[localPlayerId].bet += betValue;
            Debug.Log("Bet:" + betValue);
            playersList[localPlayerId].DeductMoney(betValue);
            Debug.Log("Money: " + playersList[localPlayerId].money);

            double playerMoney = playersList[localPlayerId].money;
            double playerBet = playersList[localPlayerId].bet;
            float xPosition = playersList[localPlayerId].position.x;

            Debug.Log("Player money: " + playerMoney);

            if(PhotonOrBluetooth())
            {
                Debug.Log("Bet value: " + betValue);
                Debug.Log("Player bet: " + playersList[localPlayerId].bet);
                photonView.RPC("DisplayBet", RpcTarget.All, playerBet, xPosition);
                photonView.RPC("UpdateMoneyText", RpcTarget.All, playerMoney, xPosition);
            }

            else 
            {
                ShowBet(playerBet, xPosition);
                UpdateMoney(playerMoney, xPosition);
                message = $"SETBET,{playerBet},{localPlayerId}";
                BluetoothForAndroid.WriteMessage(message);
            }
        }
    }


    public void BetTurn()
    {
        ChipButtons.SetActive(true);
        ConfirmButton.SetActive(true);
    }



    public void ConfirmButtonClicked()
    {
        if (playersList[Globals.localPlayerId].bet == 0)
        {
            ShowText("You have to choose bet!");
        }

        else
        {
            ChipButtons.SetActive(false);
            ConfirmButton.SetActive(false);
            photonView.RPC("PlayerConfirmedBet", RpcTarget.All);
        }
    }

    [PunRPC]
    public void PlayerConfirmedBet()
    {
        playersConfirmed += 1;

        if (playersConfirmed >= playersList.Count)
        {
            EndBetingTurn();
        }
    }

    public void EndBetingTurn()
    {
        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);

        if(PhotonOrBluetooth())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DealInitialCards();
            }
        }

        HitStandTurn();
    }

    [PunRPC]
    public void GameStartRPC()
    {
        if (playersList.Count > 1)
        {
            masterButton.SetActive(false);
            currentPlayerIndex = 0;
            dealerBot.position = dealerPosition.position;
            rpcDeck = deck;
            if (methodCount < 1)
            {
                ChipButtonsAction();
            }
            BetTurn();
        }

        else
        {
            if(PhotonNetwork.IsMasterClient)
            {
                ShowText("Not enough players!");
                Debug.Log("Not enough players!");
            }
        }
    }


    public override void GameStart()
    {
        base.GameStart();

        if(PhotonOrBluetooth())
        {
            photonView.RPC("GameStartRPC", RpcTarget.AllBuffered);
        }

        else
        {
            if(playersList.Count > 1)
            {
                masterButton.SetActive(false);
                currentPlayerIndex = 0;
                dealerBot.position = dealerPosition.position;
                rpcDeck = deck;
                playerId = playersList.Keys.ElementAt(currentPlayerIndex);

                
                ShowText("After deal");


                if (methodCount < 1)
                {
                    ChipButtonsAction();
                }
                BetTurn();
                message = $"GAMESTART,{currentPlayerIndex},{dealerBot.position.x},{dealerBot.position.y},{dealerBot.position.z}";
                BluetoothForAndroid.WriteMessage(message);
            }

            else
            {
                ShowText("Not enough players!");
                Debug.Log("Not enough players!");
            }
            
        }
        
    }

    public void ShowText(string info)
    {
        btText.color = new Color(btText.color.r, btText.color.g, btText.color.b, 0);
        btText.text = info;

        btText.DOFade(1, 0.5f)
            .OnComplete(() =>
            {
                btText.DOFade(0, 0.5f).SetDelay(2f);
            });
    }


    [PunRPC]
    public void DealCardToPlayer(string pId, int cardId)
    {
        if (!playersList.ContainsKey(pId))
        {
            Debug.LogError($"Player ID {pId} not found in playersList!");
            return;
        }

        Card card = rpcDeck.GetCardById(cardId);
        Debug.Log("RCP card value: " + card.value);
        playersList[pId].AddCardToHand(card);
        Debug.Log($"Player {playerId} hand size before: {playersList[playerId].playerCards.Count}");
        Debug.Log($"Player {playerId} object count before: {playersList[playerId].cardsObjects.Count}");
        StartCoroutine(cardDealer.DealCards(playersList[pId], cardId, true));
        Debug.Log($"Player {playerId} hand size after: {playersList[playerId].playerCards.Count}");
        Debug.Log($"Player {playerId} object count after: {playersList[playerId].cardsObjects.Count}");
    }

    [PunRPC]
    public void DealCardToDealer(int cardId, bool showDealerCard)
    {
        Card card = rpcDeck.GetCardById(cardId);
        Debug.Log("RCP card value: " + card.value);
        dealerBot.AddCardToHand(card);
        StartCoroutine(cardDealer.DealCards(dealerBot, cardId, showDealerCard));
    }

    


    private void DealInitialCards()
    {
        if (PhotonOrBluetooth())
        {
            if(PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < playersList.Count; j++)
                    {
                        playerId = playersList.Keys.ElementAt(j);
                        Card drawnCard = deck.DrawCard();

                        photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                        playersScore[j] = playersList[playerId].score;
                    }
                    Card dealerCard = deck.DrawCard();
                    if (i == 1)
                    {
                        showDealerCard = false;
                    }
                    photonView.RPC("DealCardToDealer", RpcTarget.AllBuffered, dealerCard.id, showDealerCard);

                }
            }
        }

        

    }


    [PunRPC]
    public void PlayerHit(string playerIndex)
    {
        if (PhotonNetwork.LocalPlayer.NickName == playerIndex)
        {
            Debug.Log("Skipping duplicate RPC for local player");
            return;
        }


        Debug.Log("Rpc player id: " + playerIndex);
        //playersList[playerIndex].score = playersScore[currentPlayerIndex];
        Card drawnCard = deck.DrawCard();
        Debug.Log("Player score: " + playersList[playerIndex].score);
        photonView.RPC("DealCardToPlayer", RpcTarget.All, playerId, drawnCard.id);
        score = playersList[playerIndex].score;
        Debug.Log(" Hit Score: " + playersList[playerIndex].score);
        if (playersList[playerIndex].Busted(score))
        {
            Debug.Log("Busted");
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position);
            StandBtnClicked();
        }
    }

    


    public void Hit()
    {
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log("Hit player id: " + playerId);
        Debug.Log("Player score bhit: " + playersList[playerId].score);
        Debug.Log("Dealer score: " + dealerBot.score);
        photonView.RPC("PlayerHit", RpcTarget.AllViaServer, playerId);
    }

    [PunRPC]
    public void UpdateTurn(int nextIndex)
    {
        currentPlayerIndex = nextIndex;
        HitStandTurn();
    }

    public void HitStandTurn()
    {
        Debug.Log("HitStandTurn");
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log(playerId);
        Debug.Log(Globals.localPlayerId);
        //HitStandBtns.SetActive(currentPlayerIndex == PhotonNetwork.LocalPlayer.ActorNumber - 1);
        HitStandBtns.SetActive(playerId == Globals.localPlayerId);
        //playersList[playerId].score = playersScore[currentPlayerIndex];
        Debug.Log("player score: " + playersList[playerId].score);
    }


    public IEnumerator Stand()
    {
        yield return new WaitForSeconds(3);
        
    }

    public void StandBtnClicked()
    {
        Debug.Log("Stand player score: " + playersList[playerId].score);
        StartCoroutine(Stand());
        HitStandBtns.SetActive(false);
        currentPlayerIndex += 1;
        if (currentPlayerIndex < playersList.Count)
        {
            photonView.RPC("UpdateTurn", RpcTarget.AllViaServer, currentPlayerIndex);
        }

        else
        {
            currentPlayerIndex = 0;
            HitStandBtns.SetActive(false);
            photonView.RPC("DealerTurn", RpcTarget.All);
        }
    }

    [PunRPC]
    public void DealerTurn()
    {
        StartCoroutine(DealerTurnCoroutine());
    }

    private IEnumerator DealerTurnCoroutine()
    {
        cardDealer.RenderHiddenCard(dealerBot.playerCards[1].id, dealerBot.cardsObjects[1]);
        Debug.Log("Dealer score: " + dealerBot.score);

        Card drawnCard;
        while (dealerBot.score < 17)
        {
            drawnCard = deck.DrawCard();
            photonView.RPC("DealCardToDealer", RpcTarget.AllViaServer, drawnCard.id, true);
            yield return new WaitForSeconds(0.3f);
        }
        Debug.Log("Dealer turn");
        if (dealerBot.Busted(dealerBot.score))
        {
            signIndex = 0;
            photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, dealerBot.position);
            Debug.Log("Dealer score: " + dealerBot.score);
            StartCoroutine(Result());
        }

        else
        {
            Debug.Log("Dealer score br: " + dealerBot.score);
            Debug.Log("Dealer cards count br: " + dealerBot.playerCards.Count);
            Debug.Log("Player score br: " + playersList[playerId].score);
            Debug.Log("Player cards count br: " + playersList[playerId].playerCards.Count);
            StartCoroutine(Result());
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
            //ShowAndHideSign(signToShow, position);
            StartCoroutine(ShowAndHideSign(signToShow, position));
        }
    }

    //RPC
    private IEnumerator ShowAndHideSign(GameObject sign, Vector3 position)
    {
        yield return new WaitForSeconds(3);
        
        sign.transform.position = position;
        sign.transform.SetAsLastSibling();
        sign.SetActive(true);

        
        sign.GetComponent<CanvasGroup>().alpha = 0; 

        
        sign.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); 
        sign.GetComponent<CanvasGroup>().DOFade(1, 0.5f); 

        
        DOVirtual.DelayedCall(3.0f, () =>
        {
            sign.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack); 
            sign.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                sign.SetActive(false); 
            });
        });

        yield return new WaitForSeconds(5);
    }



    public IEnumerator Result()
    {
        yield return new WaitForSeconds(2);
        double betMoney = 0;
        Debug.Log("Result dealer score: " + dealerBot.score);
        Debug.Log("Dealer cards count: " + dealerBot.playerCards.Count);

        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            Debug.Log("Player cards count: " + playersList[playerId].playerCards.Count);
            Debug.Log(playerId + " score: " + score);
            Debug.Log("Player score: " + playersList[playerId].score);
            Debug.Log("PlayerScore:  " + playersScore[0]);

            if (playersList[playerId].score > dealerBot.score)
            {
                Debug.Log("I'm here!");
                betMoney += playersList[playerId].bet * 2;
                playersList[playerId].AddMoney(betMoney);
                signIndex = 1;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                yield return new WaitForSeconds(2);
            }

            if (playersList[playerId].score < dealerBot.score && !dealerBot.Busted(dealerBot.score))
            {
                Debug.Log("I'm here!");
                signIndex = 2;
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                yield return new WaitForSeconds(2);
            }

            if (playersList[playerId].score == dealerBot.score && !playersList[playerId].Busted(score) && !dealerBot.Busted(dealerBot.score))
            {

                if (playersList[playerId].IfBlackJack() && !dealerBot.IfBlackJack())
                {
                    Debug.Log("I'm here!");
                    betMoney = playersList[playerId].bet + playersList[playerId].bet * 1.5;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 1;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(2);
                }

                if (dealerBot.IfBlackJack() && !playersList[playerId].IfBlackJack())
                {
                    Debug.Log("I'm here!");
                    signIndex = 2;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);

                    yield return new WaitForSeconds(2);
                }

                if (playersList[playerId].IfBlackJack() && dealerBot.IfBlackJack())
                {
                    Debug.Log("I'm here!");
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(2);
                }

                else
                {
                    Debug.Log("I'm here!");
                    betMoney += playersList[playerId].bet;
                    playersList[playerId].AddMoney(betMoney);
                    signIndex = 3;
                    photonView.RPC("ShowAndHideResultSign", RpcTarget.AllViaServer, signIndex, playersList[playerId].position + signPosition);
                    photonView.RPC("UpdateMoneyText", RpcTarget.AllViaServer, playersList[playerId].money, playersList[playerId].position.x);

                    yield return new WaitForSeconds(2);
                }
            }

        }

        yield return new WaitForSeconds(2);
        photonView.RPC("EndGameRPC", RpcTarget.All);
    }

    [PunRPC]
    public void EndGameRPC()
    {
        EndGame();
    }

    
    public override void EndGame()
    {
        base.EndGame();
        Debug.Log("EndGame");
        deck = rpcDeck;
        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            playersList[playerId].ResetHand();
            playersList[playerId].score = 0;
            playersList[playerId].bet = 0;
            pointsText[i].text = "";
            playersScore[i] = 0;

            for(int j=0; j< playersList[playerId].cardsObjects.Count; j++)
            {
                StartCoroutine(cardDealer.DestroyCards(playersList[playerId].cardsObjects[j]));
            }
            playersList[playerId].cardsObjects.Clear();
            Debug.Log("Player objects: " + playersList[playerId].cardsObjects.Count);
        }

        dealerBot.ResetHand();
        dealerBot.score = 0;
        showDealerCard = true;
        playersConfirmed = 0;
        score = 0;

        for(int i=0; i<dealerBot.cardsObjects.Count; i++)
        {
            StartCoroutine(cardDealer.DestroyCards(dealerBot.cardsObjects[i]));
        }
        Debug.Log("Dealer objects: " + dealerBot.cardsObjects.Count);

        dealerBot.cardsObjects.Clear();
        Debug.Log("Dealer objects: " + dealerBot.cardsObjects.Count);

        for (int i=0; i<betText.Length; i++)
        {
            betText[i].text = "";
        }

        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID);

        if (PhotonNetwork.IsMasterClient)
        {
            masterButton.SetActive(true);
        }
    }

    void OnRecivedStringMessage(string message)
    {
        if (message.StartsWith("ADDPLAYER"))
        {
            string[] parts = message.Split(',');
            if (parts.Length < 6)
            {
                Debug.LogWarning("Niepoprawny format wiadomości ADDPLAYER");
                btText.text = "Wrong Format!";
                return;
            }

            float x = float.Parse(parts[1]);
            float y = float.Parse(parts[2]);
            float z = float.Parse(parts[3]);
            string playerName = parts[4];
            int id = int.Parse(parts[5]);

            if (!playersList.ContainsKey(playerName))
            {
                Player player = new(playerName);
                playersList.Add(playerName, player);
                playersList[playerName].position = new Vector3(x, y, z);
            }
            else
            {
                Debug.LogWarning($"Player " + playerName+ "already exists!");
            }

            currentPlayerIndex = id + 1;

        }

        if(message.StartsWith("HIDEBUTTON"))
        {
            string[] parts = message.Split(',');
            if (parts.Length < 2)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            string btnName = parts[1];

            GameObject newbutton = GameObject.Find(btnName);
            if (newbutton != null && newbutton.activeSelf)
            {
                newbutton.SetActive(false);
            }

        }

        if(message.StartsWith("GAMESTART"))
        {
            string[] parts = message.Split(',');
            if (parts.Length < 5)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            int id = int.Parse(parts[1]);
            float x = float.Parse(parts[2]);
            float y = float.Parse(parts[3]);
            float z = float.Parse(parts[4]);



            currentPlayerIndex = id;
            dealerBot.position = new Vector3(x, y, z);
            rpcDeck = deck;
            ChipButtonsAction();
            BetTurn();
        }

        if(message.StartsWith("SETBET"))
        {
            string[] parts = message.Split(",");
            if(parts.Length < 3)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            double playerBet = double.Parse(parts[1]);
            string localPlayerId = parts[2];

            playersList[localPlayerId].bet += playerBet;
            playersList[localPlayerId].DeductMoney(playerBet);


        }

        if (message.StartsWith("SHOWBET"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 3)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            string playerBet = parts[1];
            int id = int.Parse(parts[2]);

            betText[id].text = playerBet;
        }

        if (message.StartsWith("UPDATEMONEY"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 3)
            {
                Debug.LogWarning("Niepoprawny format wiadomości UPDATEMONEY");
                btText.text = "Wrong Format!";
                return;
            }

            string playerMoney = parts[1];
            int id = int.Parse(parts[2]);

            MoneyText[id].text = playerMoney;
        }


        if (message.StartsWith("CONFIRM"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 2)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            int confirmed = int.Parse(parts[1]);

            playersConfirmed = confirmed;

            if (playersConfirmed >= playersList.Count)
            {
                EndBetingTurn();
            }
        }

        /*if (message.StartsWith("DEALCARDS"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 1)
            {
                Debug.LogWarning("Niepoprawny format wiadomości HIDEBUTTON");
                btText.text = "Wrong Format!";
                return;
            }

            DealInitialCards();
        }*/

        if (message.StartsWith("DEALCARDSPLAYER"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 3)
            {
                Debug.LogWarning("Niepoprawny format wiadomości DEALCARDSPLAYER");
                btText.text = "Wrong Format!";
                ShowText(btText.text);
                return;
            }

            string id = parts[1];
            int cardId = int.Parse(parts[2]);

            Card card = rpcDeck.GetCardById(cardId);
            Debug.Log("RCP card value: " + card.value);
            playersList[id].AddCardToHand(card);
            //StartCoroutine(cardDealer.DealCards(playersList[id], cardId, true));
            //cardDealer.Deal(playersList[playerId], cardId, true);

        }

        if (message.StartsWith("DEALCARDSDEALER"))
        {
            string[] parts = message.Split(",");
            if (parts.Length < 3)
            {
                Debug.LogWarning("Niepoprawny format wiadomości DEALCARDSDEALER");
                btText.text = "Wrong Format Dealer!";
                ShowText(btText.text);
                return;
            }

            int id = int.Parse(parts[1]);
            bool showCard = bool.Parse(parts[2]);

            Card card = rpcDeck.GetCardById(id);
            Debug.Log("RCP card value: " + card.value);
            dealerBot.AddCardToHand(card);
            //StartCoroutine(cardDealer.DealCards(dealerBot, id, showDealerCard));
            //cardDealer.Deal(dealerBot, id, showDealerCard);

        }

        if (message.StartsWith("ENDGAME"))
        {
            EndGame();
        }

    }

    void OnDestroy()
    {
        if(GameModeManager.Instance.selectedMode == GameModeManager.GameMode.Bluetooth)
        {
            BluetoothForAndroid.ReceivedStringMessage -= OnRecivedStringMessage;
        }
    }


}
