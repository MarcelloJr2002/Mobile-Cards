using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using SVSBluetooth;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Poker : BaseGameManager
{
    public Transform[] playerPositions;
    public Button[] positionButtons;
    public CardDealer cardDealer;
    public Deck deck;
    public Deck rpcDeck;
    public int currentPlayerIndex = 0;
    public Text[] betText;
    public Text[] MoneyText;
    public Text infoText;
    public string playerId = "";
    private Dictionary<string, Player> playersList = new Dictionary<string, Player>();
    private PhotonView photonView;
    private int pot = 0;
    private int bet = 0;
    private string[] pPosition = { "SM", "BB", "UTG", "BTN" };
    public GameObject[] positionSign;
    //public GameObject buttons;
    public GameObject[] actionButtons;
    public GameObject[] resultSign;
    public GameObject raiseButton;
    public InputField betInputField;
    public GameObject masterButton;
    public Text potText;
    private string gameMoment = "";
    private List<Card> cardsOnTable = new();
    //public RectTransform uiParent;
    private Vector3 targetPosition;
    public Transform tablePosition;
    private bool viewChanged = false;
    private bool isLocalPlayer = false;
    private bool raiseCondition = false;
    private bool ifChecked = false;
    private bool ifBet = false;
    public PokerHandEvaluator handEvaluator;
    Dictionary<string, PokerHandEvaluator.HandResult> playerBestHands = new Dictionary<string, PokerHandEvaluator.HandResult>();
    private int smallBlind = 5;
    private int bigBlind = 10;
    private string message;
    private int signIndex = 0;
    private Player winner;
    private Player raiser;
    public List<GameObject> tableCards = new List<GameObject>();
    private int compareResult;



    private void Start()
    {
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

    [PunRPC]
    public void AddPlayer(string playerName, Vector3 position)
    {
        Player newPlayer = new(PhotonNetwork.LocalPlayer);
        if (playersList.ContainsKey(playerName))
        {
            playerName += " 1";
        }
        playersList.Add(playerName, newPlayer);
        playersList[playerName].position = position;
        playersList[playerName].pokerPosition = pPosition[playersList.Count - 1];
        playersList[playerName].name = playerName;

        if (playersList[playerName].pokerPosition == "SM")
        {
            positionSign[0].transform.position = position + new Vector3(100, 0, 0);
            positionSign[0].SetActive(true);
        }

        if (playersList[playerName].pokerPosition == "BB")
        {
            positionSign[1].transform.position = position + new Vector3(100, 0, 0);
            positionSign[1].SetActive(true);
        }
        Debug.Log("Player position: " + playersList[playerName].pokerPosition);
        Debug.Log("Player name: " + playerName);
    }

    public void PositionButtonClicked(GameObject button)
    {
        if (PhotonOrBluetooth())
        {
            PhotonNetwork.LocalPlayer.NickName = Globals.localPlayerId;

            targetPosition = button.transform.position;
            Debug.Log(targetPosition);

            photonView.RPC("AddPlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, button.transform.position);


            photonView.RPC("HideButton", RpcTarget.AllBuffered, button.name);

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

        if(PhotonNetwork.IsMasterClient)
        {
            masterButton.SetActive(true);
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

    

    public override void GameStart()
    {
        base.GameStart();
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
                betText[i].text = betValue.ToString() + "$";
                Debug.Log(betText[i].text);
                if(!PhotonOrBluetooth())
                {
                    message = $"SHOWBET,{betText[i].text},{i}";
                    BluetoothForAndroid.WriteMessage(message);
                }
                break;
            }
        }
    }

    [PunRPC]
    public void TestMethod()
    {
        playersList[PhotonNetwork.LocalPlayer.NickName].position = targetPosition;
        rpcDeck = deck;
        gameMoment = "PreFlop";
        pot = 0;
        if (playersList.Count > 1)
        {
            for(int i=0; i<playersList.Count; i++)
            {
                playerId = playersList.Keys.ElementAt(i);

                if (playersList[playerId].pokerPosition == "SM")
                {
                    playersList[playerId].bet = smallBlind;
                    pot += smallBlind;
                }

                if (playersList[playerId].pokerPosition == "BB")
                {
                    playersList[playerId].bet = bigBlind;
                    pot += bigBlind;
                }

                double playerMoney = playersList[playerId].money;
                double playerBet = playersList[playerId].bet;
                float xPosition = playersList[playerId].position.x;
                DisplayBet(playerBet, xPosition);
                UpdateMoney(playerMoney, xPosition);
            }
            potText.text = pot.ToString();
            bet = smallBlind;
            
            DealInitialCards();
        }

        else
        {
            ShowText("Not enough players!");
            Debug.Log("Not enough players!");
        }
    }

    public void StartGame()
    {
        if (PhotonOrBluetooth())
        {
            photonView.RPC("TestMethod", RpcTarget.AllBuffered);
        }
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
        Debug.Log("RCP card value: " + card.value + " " + card.cardType + " " + card.cardColor);
        playersList[pId].AddCardToHand(card);
        isLocalPlayer = (pId == Globals.localPlayerId);
        StartCoroutine(cardDealer.DealCards(playersList[pId], cardId, isLocalPlayer));
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

                    Debug.Log("Card value: " + drawnCard.value + " " + drawnCard.cardType + " " + drawnCard.cardColor);
                    Debug.Log(drawnCard.ToString());

                    //playersList[playerId].AddCardToHand(drawnCard);
                    photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                }
            }

            PreFlop();

        }
    }

    public void ShowButtons()
    {
        //buttons.SetActive(true);

        // Ukrywanie przycisków w zależności od stanu gry
        if (gameMoment == "PreFlop")
        {
            if (playersList[playerId].pokerPosition == "BB")
            {
                // Big Blind może checkować, jeśli nie było raisa
                actionButtons[2].SetActive(!raiseCondition); // Pokaż "Check", jeśli nie było Raise
                actionButtons[1].SetActive(raiseCondition); //call
            }
            else
            {
                actionButtons[2].SetActive(false); // Inni gracze nie mogą Check bez opłacenia BB
                actionButtons[1].SetActive(true); //call
            }
        }
        else
        {
            actionButtons[1].SetActive(raiseCondition); //call
            actionButtons[2].SetActive(!raiseCondition); //check
        }

        actionButtons[0].SetActive(true); //fold
        actionButtons[3].SetActive(true); //raise

        betInputField.gameObject.SetActive(true);
    }


    public void HideButtons()
    {
        //buttons.SetActive(false);
        foreach (var button in actionButtons)
        {
            button.SetActive(false);
        }
        betInputField.gameObject.SetActive(false);
    }

    public void PreFlop()
    {
        foreach (var players in playersList.Values)
        {
            Debug.Log("Player cards: " + players.playerCards.Count());
            Debug.Log("Player position: " + players.pokerPosition);
        }


        for (int i = 0; i < playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);

            if (playersList.Count == 2)
            {
                if (playersList[playerId].pokerPosition == "SM")
                {
                    currentPlayerIndex = i;
                    ShowButtons();
                    actionButtons[2].SetActive(false);
                    break;
                }
            }

            if (playersList.Count == 3)
            {
                if (playersList[playerId].pokerPosition == "BTN")
                {
                    currentPlayerIndex = i;
                    ShowButtons();
                    break;
                }
            }

            if (playersList.Count == 4)
            {
                if (playersList[playerId].pokerPosition == "UTG")
                {
                    currentPlayerIndex = i;
                    ShowButtons();
                    break;
                }
            }
        }
    }

    [PunRPC]
    public void UpdatePosition()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);

        Debug.Log("Player: " + playerId + " position: " + playersList[playerId].pokerPosition);

        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {   
            ShowButtons();
        }
    }

    [PunRPC]
    public void UpdateTurn(string moment)
    {
        bet = 0;
        ifChecked = false;
        raiseCondition = false;
        currentPlayerIndex = 0;

        switch (moment)
        {
            case "PreFlop":
                gameMoment = "Flop";
                Flop();
                break;

            case "Flop":
                gameMoment = "Turn";
                Turn();
                break;

            case "Turn":
                gameMoment = "River";
                River();
                break;

            case "River":
                gameMoment = "ShowDown";
                StartCoroutine(ShowDown());
                break;

            default:
                Debug.LogWarning("Nieznany moment gry: " + moment);
                break;
        }
    }

    [PunRPC]
    public void CallRPC()
    {
        playersList[playerId].DeductMoney(bet);
        pot += bet;
        potText.text = pot.ToString();
    }

    public void Call()
    {
        StartCoroutine(CallCoroutine());
    }

    public IEnumerator CallCoroutine()
    {
        HideButtons();
        photonView.RPC("CallRPC", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(0.3f);

        double playerMoney = playersList[playerId].money;
        double playerBet = playersList[playerId].bet;
        float xPosition = playersList[playerId].position.x;
        photonView.RPC("DisplayBet", RpcTarget.AllBuffered, playerBet, xPosition);
        yield return new WaitForSeconds(0.3f);

        photonView.RPC("UpdateMoneyText", RpcTarget.AllBuffered, playerMoney, xPosition);
        yield return new WaitForSeconds(0.3f);

        if (playersList[playerId].pokerPosition == "BB")
        {
            photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, gameMoment);
            yield return new WaitForSeconds(0.3f);
        }

        else
        {
            if (gameMoment == "PreFlop" && raiseCondition == false)
            {
                photonView.RPC("UpdatePosition", RpcTarget.AllBuffered);
                yield return new WaitForSeconds(0.3f);
            }

            if (raiseCondition == true)
            {
                Debug.Log("Called the raise!");
                photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, gameMoment);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    [PunRPC]
    public void RaiseRPC(string betAmount)
    {
        if (int.TryParse(betAmount, out int raiseValue))
        {
            bet = Mathf.Max(bet, raiseValue); // Ustaw nowy bet, jeśli jest większy niż obecny
            pot += bet;
            potText.text = pot.ToString();
            playersList[playerId].DeductMoney(bet);
            raiseCondition = true; // Raise został wykonany
            //playersList[playerId].ifRaised = true;
            raiser = playersList[playerId];
        }
        else
        {
            ShowText("Bet must be integer!");
            Debug.Log("Niepoprawna wartość Raise!");
        }
    }

    public void Raise()
    {
        StartCoroutine(RaiseCoroutine());
    }

    public IEnumerator RaiseCoroutine()
    {
        HideButtons();
        photonView.RPC("RaiseRPC", RpcTarget.AllViaServer, betInputField.text);
        yield return new WaitForSeconds(0.3f);

        double playerMoney = playersList[playerId].money;
        double playerBet = playersList[playerId].bet;
        float xPosition = playersList[playerId].position.x;
        photonView.RPC("DisplayBet", RpcTarget.AllBuffered, playerBet, xPosition);
        yield return new WaitForSeconds(0.3f);

        photonView.RPC("UpdateMoneyText", RpcTarget.AllBuffered, playerMoney, xPosition);
        yield return new WaitForSeconds(0.3f);

        //photonView.RPC("UpdateTurn", RpcTarget.AllViaServer, gameMoment);
        photonView.RPC("UpdatePosition", RpcTarget.AllBuffered);
        yield return new WaitForSeconds(0.3f);
    }



    [PunRPC]
    public void FoldRPC()
    {
        playersList[playerId].ifFolded = true;
        foreach (var player in playersList.Values)
        {
            player.ResetHand();
            player.bet = 0;

            foreach (var cardObject in player.cardsObjects)
            {
                StartCoroutine(cardDealer.DestroyCards(cardObject));
            }

            player.playerCards.Clear();
            player.cardsObjects.Clear();

            if(player.ifFolded == false)
            {
                winner = player;
            }
        }

        EndGame();

    }

    public void Fold()
    {
        HideButtons();
        photonView.RPC("FoldRPC", RpcTarget.AllBuffered);
        //photonView.RPC("UpdateTurn", RpcTarget.AllViaServer, gameMoment);
        //photonView.RPC("UpdatePosition", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void CheckRPC()
    {
        ifChecked = true; // Gracz wykonał Check
    }

    public void Check()
    {
        StartCoroutine(CheckCoroutine());
    }

    public IEnumerator CheckCoroutine()
    {
        HideButtons();
        photonView.RPC("CheckRPC", RpcTarget.AllBuffered);
        yield return new WaitForSeconds(0.3f);
        if (playersList[playerId].pokerPosition == "BB")
        {
            photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, gameMoment);
            yield return new WaitForSeconds(0.3f);
        }

        else
        {
            photonView.RPC("UpdatePosition", RpcTarget.AllBuffered);
            yield return new WaitForSeconds(0.3f);
        }
        Debug.Log("Check!");
    }


    [PunRPC]
    public void DealCardsOnTable(int howMany, int cardId)
    {
        Card card = rpcDeck.GetCardById(cardId);
        cardsOnTable.Add(card);

        Debug.Log("Flop card rpc: " + card.value + " " + card.cardType + " " + card.cardColor);

        Debug.Log("Flop card rpc: " + card.id);

        StartCoroutine(cardDealer.PokerDealer(card.id, tablePosition.position, howMany, tableCards));
    }


    public void Flop()
    {
        gameMoment = "Flop";

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Flop");

            for(int i=0; i<3; i++)
            {
                Card card = deck.DrawCard();
                cardsOnTable.Add(card);

                Debug.Log("Flop card: " + card.value + " " + card.cardType + " " + card.cardColor);
                Debug.Log("Flop card: " + card.id);

                photonView.RPC("DealCardsOnTable", RpcTarget.AllBuffered, i, card.id);
            }
        }

        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            ShowButtons();
        }

        //photonView.RPC("UpdatePosition", RpcTarget.AllViaServer);


    }

    public void Turn()
    {
        gameMoment = "Turn";

        if (PhotonNetwork.IsMasterClient)
        {
            Card card = deck.DrawCard();
            cardsOnTable.Add(card);

            photonView.RPC("DealCardsOnTable", RpcTarget.AllBuffered, 3, card.id);
        }

        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            ShowButtons();
        }

        //photonView.RPC("UpdatePosition", RpcTarget.AllViaServer);
    }

    public void River()
    {
        gameMoment = "River";

        if (PhotonNetwork.IsMasterClient)
        {
            Card card = deck.DrawCard();
            cardsOnTable.Add(card);

            photonView.RPC("DealCardsOnTable", RpcTarget.AllBuffered, 4, card.id);
        }

        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        if (PhotonNetwork.LocalPlayer.NickName == playerId)
        {
            ShowButtons();
        }

        //photonView.RPC("UpdatePosition", RpcTarget.AllViaServer);
    }

    public void AssignResultSign(string description)
    {
        switch(description)
        {
            case "High Card":
                signIndex = 0;
                break;
            case "One Pair":
                signIndex = 1; 
                break;
            case "Two Pair":
                signIndex = 2;
                break;
            case "Three of a Kind":
                signIndex = 3;
                break;
            case "Straight":
                signIndex = 4;
                break;
            case "Flush":
                signIndex = 5;
                break;
            case "Full House":
                signIndex = 6;
                break;
            case "Four of a Kind":
                signIndex = 7;
                break;
            case "Straight Flush":
                signIndex = 8;
                break;
            case "Royal Flush":
                signIndex = 9;
                break;
        }
    }

    [PunRPC]
    public void ShowDownRPC()
    {
        for(int i=0; i<playersList.Count(); i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            var bestHand = PokerHandEvaluator.FindBestHand(playersList[playerId], cardsOnTable);

            if(!playerBestHands.ContainsKey(playerId))
            {
                playerBestHands.Add(playerId, bestHand);
            }

            Debug.Log($"{playerId} ma najlepszy układ: {bestHand.Description}");
        }

        var winningHand = playerBestHands[playerId];
        //int compareResult;
        playerId = playersList.Keys.ElementAt(0);
        winner = playersList[playerId];

        for (int i=0; i<playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            compareResult = PokerHandEvaluator.CompareHands(winningHand, playerBestHands[playerId]);
            if(compareResult == -1)
            {
                winningHand = playerBestHands[playerId];
                winner = playersList[playerId];
            }

            if(compareResult == 0)
            {
                winner = playersList[playerId];
            }
        }
        if(compareResult != 0)
        {
            AssignResultSign(winningHand.Description);
            StartCoroutine(ShowAndHideSign(signIndex, winner.position));
        }

        else
        {
            ShowText("Draw");
        }

        EndGame();
    }

    private IEnumerator ShowAndHideSign(int index, Vector3 position)
    {
        yield return new WaitForSeconds(1);
        GameObject sign = resultSign[index];
        
        sign.transform.position = position + new Vector3(0, 100, 0);
        sign.transform.SetAsLastSibling();
        sign.SetActive(true);

        if (sign.GetComponent<CanvasGroup>() == null)
        {
            sign.AddComponent<CanvasGroup>();
        }

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

        yield return new WaitForSeconds(2);
    }

    [PunRPC]
    public void WinnerAnimation()
    {
        Debug.Log("WinnerAnimation!");
        Debug.Log(winner == null);
        StartCoroutine(ShowAndHideSign(signIndex, winner.position));
    }

    [PunRPC]
    public void RevealCards(string Id)
    {
        cardDealer.RenderHiddenCard(playersList[Id].playerCards[1].id, playersList[Id].cardsObjects[1]);
        cardDealer.RenderHiddenCard(playersList[Id].playerCards[0].id, playersList[Id].cardsObjects[0]);
    }


    public IEnumerator ShowDown()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RevealCards", RpcTarget.Others, Globals.localPlayerId);
            yield return new WaitForSeconds(0.3f);
            photonView.RPC("ShowDownRPC", RpcTarget.AllBuffered);
        }
    }

    public override void EndGame()
    {
        base.EndGame();
        Debug.Log("EndGame");
        deck = rpcDeck;
        bet = 0;
        ifChecked = false;
        raiseCondition = false;
        currentPlayerIndex = 0;

        for (int i = 0; i < playersList.Count; i++)
        {
            playerId = playersList.Keys.ElementAt(i);
            playersList[playerId].ResetHand();
            playersList[playerId].score = 0;
            playersList[playerId].bet = 0;
            

            for (int j = 0; j < playersList[playerId].cardsObjects.Count; j++)
            {
                StartCoroutine(cardDealer.DestroyCards(playersList[playerId].cardsObjects[j]));
            }
            playersList[playerId].cardsObjects.Clear();
            Debug.Log("Player objects: " + playersList[playerId].cardsObjects.Count);
        }

        for (int i = 0; i < betText.Length; i++)
        {
            betText[i].text = "";
        }

        foreach(var card in tableCards)
        {
            StartCoroutine(cardDealer.DestroyCards(card));
        }
        tableCards.Clear();
        playerBestHands.Clear();

        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID);

        if (PhotonNetwork.IsMasterClient)
        {
            masterButton.SetActive(true);
        }
    }

    void OnRecivedStringMessage(string message)
    {

    }
}
