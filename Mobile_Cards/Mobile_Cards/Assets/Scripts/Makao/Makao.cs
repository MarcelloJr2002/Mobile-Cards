using Photon.Pun;
using SVSBluetooth;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static Makao;
using static Card;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;

public class Makao : BaseGameManager
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
    public GameObject winnerSign;
    public GameObject masterButton;
    public GameObject ConfirmButton;
    public GameObject drawCardButton;
    public int signIndex = 0;
    public string playerId = "";
    private Dictionary<string, Player> playersList = new Dictionary<string, Player>();
    private PhotonView photonView;
    public Deck deck;
    public Deck rpcDeck;
    private Vector3 signPosition = new Vector3(0, 150, 0);
    public Text btText;
    public Text idText;
    private bool isLocalPlayer = false;
    public Transform tablePosition;
    public int maxVisibleCards = 6;
    private List<Card> tableCards = new List<Card>();
    private List<GameObject> cardsOnTable = new List<GameObject>();
    private Card currentCardOnTable;
    private string currentSuit;
    private int cardsToDraw = 0;
    private string nextId;
    public Transform cardsContainer1;
    public Transform cardsContainer2;
    public Transform canvasTransform;
    private Card requestedCard;
    private List<Card> selectedRequestCard = new List<Card>();
    public string requestedColor = "";
    public int requestedValue = 0;
    private bool canHandleRequest = true;
    private int demandRequest = 0;
    private int colorRequest = 0;
    private bool ifRequested = false;
    private bool firstCard;
    private int scrollOffset = 0;
    private bool listenerSet = false;

    public Dropdown extraCardsDropdown;
    public GameObject cardPrefab;
    public GameObject selectedCardHighlightPrefab;
    public PanelManager panelManager;
    public GameObject panel;
    public Text[] cardsCountText;
    public Button LeftArrowBtn;
    public Button RightArrowBtn;
    public Button LeftArrowBtn2;
    public Button RightArrowBtn2;

    private List<GameObject> displayedCards = new List<GameObject>();
    private List<Card> selectedCards = new List<Card>();
    private List<GameObject> selectedCardsObjects = new List<GameObject>();
    private Dictionary<GameObject, GameObject> cardHighlights = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> requestHighlights = new Dictionary<GameObject, GameObject>();
    private Dictionary<Card, GameObject> dropdownHighlights = new Dictionary<Card, GameObject>();

    public List<Card> extraCards = new List<Card>();
    public CardClickHandler clickHandler;
    private Request request;

    private enum Request
    {
        None,
        Draw,
        Stand,
        Demand,
        ChangeColor
    }


    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void SetArrowsListener(Player player)
    {
        Transform parentTransform = (player.position == positionButtons[0].transform.position) ? cardsContainer1 : cardsContainer2;

        if(player.position == positionButtons[0].transform.position)
        {
            LeftArrowBtn2.onClick.AddListener(() => OnLeftArrowClick(player, parentTransform));
            RightArrowBtn2.onClick.AddListener(() => OnRightArrowClick(player, parentTransform));
        }

        else
        {
            LeftArrowBtn.onClick.AddListener(() => OnLeftArrowClick(player, parentTransform));
            RightArrowBtn.onClick.AddListener(() => OnRightArrowClick(player, parentTransform));
        }
        listenerSet = true;
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
        playersList[playerName].name = playerName;
    }

    public void PositionButtonClicked(GameObject button)
    {

        if (button == null)
        {
            Debug.LogError("Argument 'button' jest null! Sprawdź konfigurację OnClick w inspektorze.");
            return;
        }

        Debug.Log($"Button przekazany do PositionButtonClicked: {button.name}");

        if(PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogError("Photon player nie istnieje!");
            return;
        }

        if (Globals.localPlayerId == null)
        {
            Debug.LogError("localplayerId nie istnieje!");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = Globals.localPlayerId;


        photonView.RPC("AddPlayer", RpcTarget.AllBuffered, Globals.localPlayerId, button.transform.position);


        photonView.RPC("HideButton", RpcTarget.AllBuffered, button.name);

        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            foreach (var positionBtn in positionButtons)
            {
                positionBtn.gameObject.SetActive(false);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            masterButton.SetActive(true);
        }
    }

    [PunRPC]
    public void HideButton(string buttonName)
    {
        GameObject newbutton = GameObject.Find(buttonName);

        if (buttonName == "Position1")
        {
            cardsCountText[0].transform.position = newbutton.transform.position + new Vector3(0, 200, 0);
            Debug.Log("Bet and text positions assigned");
        }

        else
        {
            cardsCountText[1].transform.position = newbutton.transform.position + new Vector3(0, 200, 0);
        }

        if (newbutton != null && newbutton.activeSelf)
        {
            newbutton.SetActive(false);
        }
    }

    public void ShowInfo(string info)
    {
        btText.color = new Color(btText.color.r, btText.color.g, btText.color.b, 0);
        btText.text = info;

        btText.DOFade(1, 0.5f)
            .OnComplete(() =>
            {
                btText.DOFade(0, 0.5f).SetDelay(2f);
            });
    }

    public void UpdateCardsCount(string id, float x)
    {
        for(int i=0; i<cardsCountText.Length; i++)
        {
            if (cardsCountText[i].transform.position.x == x)
            {
                cardsCountText[i].text = id + " cards: " + playersList[id].playerCards.Count;
                Debug.Log("Cards count: " + cardsCountText[i].text);
            }
        }
    }

    [PunRPC]
    public void StartGameRPC()
    {
        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        requestedValue = 0;
        requestedColor = "";

        if (!listenerSet)
        {
            Debug.Log("Add listener");
            foreach (var player in playersList.Values)
            {
                SetArrowsListener(player);
            }
        }
    }

    public override void GameStart()
    {
        base.GameStart();
        if(playersList.Count > 1)
        {
            masterButton.SetActive(false);

            if (!listenerSet)
            {
                Debug.Log("Add listener");
                foreach (var player in playersList.Values)
                {
                    SetArrowsListener(player);
                }
            }
            photonView.RPC("StartGameRPC", RpcTarget.AllBuffered);
            DealInitialCards();

            if (PhotonNetwork.IsMasterClient)
            {
                ConfirmButton.SetActive(true);
                drawCardButton.SetActive(true);
            }

            Debug.Log("Arrow null: " + LeftArrowBtn == null);
        }

        else
        {
            ShowInfo("Not enough players!");
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
        Debug.Log($"RPC: Dealing card {card.cardType} {card.cardColor} (ID: {cardId}) to player {pId}.");

        playersList[pId].AddCardToHand(card);

        bool isLocalPlayer = (pId == Globals.localPlayerId);
        StartCoroutine(cardDealer.DealCards(playersList[pId], cardId, isLocalPlayer));

        Debug.Log($"Player {pId} hand size: {playersList[pId].playerCards.Count}");

        Player currentPlayer = playersList[pId];
        if (playersList[pId].playerCards.Count > 5)
        {
 
            scrollOffset = playersList[pId].playerCards.Count - 5;
        }
        else
        {
            scrollOffset = 0;
        }

        Transform parentTransform = (playersList[playerId].position == positionButtons[0].transform.position) ? cardsContainer1 : cardsContainer2;

        int newScrollOffset = Mathf.Max(0, playersList[pId].cardsObjects.Count - 5);

        int counter = playersList[pId].playerCards.Count;

        if(counter > 6)
        {
            counter = 6;
        }

        cardDealer.RearrangeCards(currentPlayer, parentTransform, 5, newScrollOffset);

        UpdateCardsCount(pId, playersList[pId].position.x);

        if (playersList[Globals.localPlayerId].position == positionButtons[0].transform.position && pId == Globals.localPlayerId)
        {
            LeftArrowBtn2.gameObject.SetActive(playersList[playerId].playerCards.Count > 5);
            RightArrowBtn2.gameObject.SetActive(playersList[playerId].playerCards.Count > 5);
        }

        if(playersList[Globals.localPlayerId].position == positionButtons[1].transform.position && pId == Globals.localPlayerId)
        {
            LeftArrowBtn.gameObject.SetActive(playersList[playerId].playerCards.Count > 5);
            RightArrowBtn.gameObject.SetActive(playersList[playerId].playerCards.Count > 5);
        }

        //Debug.Log($"Player {pId} hand size: {playersList[pId].playerCards.Count}");
        Debug.Log($"Player {pId} object count: {playersList[pId].cardsObjects.Count}");
        UpdateCardsCount(pId, playersList[pId].position.x);
    }


    private void DealInitialCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < playersList.Count; j++)
                {
                    playerId = playersList.Keys.ElementAt(j);
                    Card drawnCard = deck.DrawCard();

                    photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                }
            }

            Card card = deck.DrawCard();
            tableCards.Add(card);
            currentCardOnTable = card;
            requestedCard = card;

            photonView.RPC("DealCardsOnTable", RpcTarget.AllBuffered, 1, card.id);
        }
    }

    [PunRPC]
    public void DealCardsOnTable(int howMany, int cardId)
    {
        Card card = rpcDeck.GetCardById(cardId);
        tableCards.Add(card);
        currentCardOnTable = card;
        requestedCard = card;
        currentSuit = card.cardColor.ToString();
        firstCard = true;

        StartCoroutine(cardDealer.PokerDealer(card.id, tablePosition.position, howMany, cardsOnTable));
    }

    [PunRPC]
    public void MoveCardFromHand(int cardId, int newScrollOffset)
    {

        Card card = rpcDeck.GetCardById(cardId);
        if (card == null)
        {
            Debug.LogError("Card not found in deck!");
            return;
        }


        tableCards.Add(card);
        currentCardOnTable = card;
        requestedCard = card;
        currentSuit = card.cardColor.ToString();


        playersList[playerId].playerCards.Remove(card);
        UpdateCardsCount(playerId, playersList[playerId].position.x);

        if (firstCard)
        {
            firstCard = false;
        }

        Debug.Log($"Card moved to table: {card.cardType} {card.cardColor} (ID: {cardId}).");


        foreach (var cardObject in selectedCardsObjects)
        {
            if (!cardObject.activeSelf)
            {
                cardObject.SetActive(true);
            }

            cardDealer.RenderHiddenCard(card.id, cardObject);

            StartCoroutine(cardDealer.MakaoDealer(cardObject, tablePosition.position, cardsOnTable));
            playersList[playerId].cardsObjects.Remove(cardObject);
        }


        scrollOffset = newScrollOffset;


        if (playersList[playerId].position == positionButtons[0].transform.position)
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer1, 5, scrollOffset);
        }
        else
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer2, 5, scrollOffset);
        }
    }



    public void OnLeftArrowClick(Player player, Transform parentTransform)
    {
        //Debug.Log("Scrolloffset: " + scrollOffset);
        if (scrollOffset > 0)
        {
            scrollOffset--;
            cardDealer.RearrangeCards(player, parentTransform, 5, scrollOffset);
        }
    }

    public void OnRightArrowClick(Player player, Transform parentTransform)
    {
        if (scrollOffset + 5 < player.cardsObjects.Count)
        {
            scrollOffset++;
            cardDealer.RearrangeCards(player, parentTransform, 5, scrollOffset);
        }
    }


    [PunRPC]
    public void SetColor(string color)
    {
        requestedColor = color;
    }

    [PunRPC]
    public void SetValue(int value)
    {
        requestedValue = value;
    }


    public void OnCardClicked(GameObject cardObject)
    {
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log($"OnCardClicked called with object: {cardObject.name}");

        if(panel.activeSelf)
        {
            panel.SetActive(false);
        }

        if (playerId == Globals.localPlayerId)
        {
            Card card = cardObject.GetComponent<CardDisplay>().Card;

            //SetRequest(card);
            if(IsCardPlayable(card))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("SetRequestRPC", RpcTarget.AllBuffered, card.id);
                }
            }

            if(request == Request.Demand && IsCardPlayable(card))
            {
                ifRequested = true;
                List<int> cardsValues = playersList[playerId].playerCards
                    .Where(card => card.value >= 6 && card.value <= 10) 
                    .Select(card => card.value)                        
                    .Distinct()                                        
                    .ToList();

                foreach (var value in cardsValues)
                {
                    Debug.Log($"Unique card value: {value}");
                }

                PlayableCardAction(card, cardObject);
                panelManager.CreateTogglesForValues(cardsValues, requestedValue);
                Debug.Log("Makao requested value: " + requestedValue);
                return;
            }

            if(request == Request.ChangeColor && IsCardPlayable(card))
            {
                ifRequested = true;
                List<Card.CardColor> cardsColors = playersList[playerId].playerCards
                    .Select(card => card.cardColor)  
                    .Distinct()                      
                    .ToList();

                foreach (var color in cardsColors)
                {
                    Debug.Log($"Unique card color: {color}");
                }

                PlayableCardAction(card, cardObject);
                panelManager.CreateTogglesForColors(cardsColors, requestedColor);
                Debug.Log("Makao requested color: " + requestedColor);
            }

            else
            {
                PlayableCardAction(card, cardObject);
            }

            
        }
    }

    public void PlayableCardAction(Card card, GameObject cardObject)
    {
        Debug.Log("Card value: " + card.value + " CardColor: " + card.cardColor);
        if (cardHighlights.ContainsKey(cardObject))
        {
            Destroy(cardHighlights[cardObject]);
            cardHighlights.Remove(cardObject);
            selectedCards.Remove(card);
            selectedCardsObjects.Remove(cardObject);
            ifRequested = false;

            if(request == Request.Draw)
            {
                if(card.value == 2)
                {
                    cardsToDraw -= 2;
                }

                if(card.value == 3)
                {
                    cardsToDraw -= 3;
                }

                if(card.cardType == CardType.King)
                {
                    cardsToDraw -= 5;
                }
            }

            if (request == Request.Demand || request == Request.ChangeColor)
            {
                photonView.RPC("SetColor", RpcTarget.AllBuffered, card.cardColor.ToString());
                photonView.RPC("SetValue", RpcTarget.AllBuffered, card.value);
            }

            request = Request.None;

            if(panel.activeSelf)
            {
                panel.SetActive(false);
            }
            Debug.Log(selectedCards.Count);
            Debug.Log("Card deselected.");
        }

        else if (IsCardPlayable(card))
        {
            Debug.Log("Is playable!");

            GameObject highlight = Instantiate(selectedCardHighlightPrefab, canvasTransform);

            highlight.transform.position = cardObject.transform.position + new Vector3(0, 200, 0);
            highlight.SetActive(true);


            cardHighlights[cardObject] = highlight;


            selectedCards.Add(card);
            selectedCardsObjects.Add(cardObject);
            Debug.Log("Card selected: " + card.value + card.cardColor);
        }

    }

    public void TakeCard()
    {
        ShowInfo("Take card");
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        if (deck.Cards.Count == 0)
        {
            deck.CreateDeck();
        }

        Card drawnCard = deck.DrawCard();

        photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);

        drawCardButton.SetActive(false);
        ConfirmButton.SetActive(false);
        NextTurn();
    }

    public void ConfirmSelectedCards()
    {
        Debug.Log("IfRequested: " + ifRequested);
        Debug.Log("DemandRequest: " + demandRequest + " ColorRequest: " + colorRequest);

        if (selectedCards.Count == 0)
        {
            Debug.Log("No cards selected.");
            ShowInfo("No cards selected!");
        }
        else
        {
            if (ifRequested)
            {
                Debug.Log("IfRequested debug value: " + requestedValue);
                panel.SetActive(false);

                if (requestedValue == 0 || requestedColor == "")
                {
                    Debug.Log("Default request");
                    requestedCard = selectedCards.First();
                    //photonView.RPC("SetRequestedValueRPC", RpcTarget.AllBuffered, requestedCard.value);
                    //photonView.RPC("SetColor", RpcTarget.AllBuffered, requestedCard.cardColor.ToString());
                }
                demandRequest = 0;
                colorRequest = 0;
                ifRequested = false;
            }

            for(int i=0; i<selectedCards.Count; i++)
            {
                photonView.RPC("MoveCardFromHand", RpcTarget.AllBuffered, selectedCards[i].id, 0);
                Debug.Log($"Card {selectedCards[i].cardType} of {selectedCards[i].cardColor} added to the table.");
            }

            Player currentPlayer = playersList[Globals.localPlayerId];


            foreach (Card card in selectedCards)
            {
                currentPlayer.playerCards.Remove(card);

                if (extraCards.Contains(card))
                {

                    extraCards.Remove(card);


                    int dropdownIndex = extraCardsDropdown.options.FindIndex(option =>
                        option.text == $"{card.cardType} {card.cardColor}" ||
                        option.text == $"{card.value} {card.cardColor}");

                    if (dropdownIndex != -1)
                    {
                        extraCardsDropdown.options.RemoveAt(dropdownIndex);
                    }
                }
            }


            selectedCards.Clear();
            foreach (var highlight in cardHighlights.Values)
            {
                Destroy(highlight);
            }
            cardHighlights.Clear();


            extraCardsDropdown.RefreshShownValue();
            ConfirmButton.SetActive(false);
            drawCardButton.SetActive(false);

            if (currentPlayer.playerCards.Count == 0)
            {
                photonView.RPC("ShowAndHideResultSign", RpcTarget.AllBuffered, currentPlayer.position);
                photonView.RPC("EndGameRPC", RpcTarget.AllBuffered);
            }
            else
            {
                NextTurn();
            }
        }
    }




    [PunRPC]
    public void UpdateTurn(int nextIndex)
    {
        currentPlayerIndex = nextIndex;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log("Current player: " + playerId);
        if (PhotonNetwork.LocalPlayer.NickName == playerId && canHandleRequest)
        {
            ConfirmButton.SetActive(true);
            drawCardButton.SetActive(true);
            //extraCardsDropdown.gameObject.SetActive(true);
        }
    }

    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
        HandleRequest(playerId);
    }

    [PunRPC]
    public void EndGameRPC()
    {
        EndGame();
    }

    public override void EndGame()
    {
        base.EndGame();

        foreach(var player in playersList.Values)
        {
            foreach(GameObject objects in player.cardsObjects)
            {
                StartCoroutine(cardDealer.DestroyCards(objects));
            }
            player.cardsObjects.Clear();
            player.playerCards.Clear();
        }

        foreach(var card in cardsOnTable)
        {
            StartCoroutine(cardDealer.DestroyCards(card));
        }
        cardsOnTable.Clear();
        tableCards.Clear();
        deck = rpcDeck;
        cardsToDraw = 0;
        PhotonNetwork.RemoveBufferedRPCs(photonView.ViewID);

        if (PhotonNetwork.IsMasterClient)
        {
            masterButton.SetActive(true);
        }
    }

    private IEnumerator ShowAndHideSign(GameObject sign, Vector3 position)
    {
        yield return new WaitForSeconds(1);

        sign.transform.position = position;
        sign.transform.SetAsLastSibling();
        sign.SetActive(true);


        sign.GetComponent<CanvasGroup>().alpha = 0;


        sign.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
        sign.GetComponent<CanvasGroup>().DOFade(1, 0.5f);


        DOVirtual.DelayedCall(3.0f, () =>
        {
            sign.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack);
            sign.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() =>
            {
                sign.SetActive(false);
            });
        });

        yield return new WaitForSeconds(2);
    }

    [PunRPC]
    public void ShowAndHideResultSign(Vector3 position)
    {
        GameObject signToShow = winnerSign;

        if (signToShow != null)
        {
            StartCoroutine(ShowAndHideSign(signToShow, position));
        }
    }

    [PunRPC]
    public void SetRequestRPC(int id)
    {
        Card card = rpcDeck.GetCardById(id);

        SetRequest(card);
    }

    public void SetRequest(Card card)
    {
        switch(card.value)
        {
            case 2:
                request = Request.Draw;
                cardsToDraw += 2;
                Debug.Log("Cards to draw: " + cardsToDraw);
                break;
            case 3:
                request = Request.Draw;
                cardsToDraw += 3;
                Debug.Log("Cards to draw: " + cardsToDraw);
                break;
            case 4:
                request = Request.Stand;
                break;
            case 11:
                request = Request.Demand;
                break;
            case 13:
                if(card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade)
                {
                    request = Request.Draw;
                    cardsToDraw += 5;
                    Debug.Log("Cards to draw: " + cardsToDraw);
                }

                else
                {
                    request = Request.None;
                }
                break;
            case 14:
                request = Request.ChangeColor;
                break;
            default:
                request = Request.None;
                break;
        }
    }

    public bool IsCardPlayable(Card card)
    {
        if(selectedCards.Count > 0)
        {
            if (card.value == selectedCards.First().value)
            {
                Debug.Log("Selected card count > 0 && true");
                return true;
            }

            else 
            {
                Debug.Log("Selected card count > 0 && false");
                Debug.Log("Card value: " + card.value);
                return false; 
            }
        }

        else
        {

            if (currentCardOnTable != null)
            {
                
                if(firstCard && (card.value == currentCardOnTable.value || card.cardColor == currentCardOnTable.cardColor || card.value == 5))
                {
                    Debug.Log("First card condition " + firstCard);
                    return true;
                }
                
                switch (currentCardOnTable.value)
                {
                    case 2: 
                        Debug.Log("Dwa");
                        return card.value == 2 ||
                               card.value == 3 && card.cardColor == currentCardOnTable.cardColor ||
                               (card.cardType == Card.CardType.King &&
                                ((card.cardColor == Card.CardColor.Heart && card.cardColor == currentCardOnTable.cardColor) ||
                                (card.cardColor == Card.CardColor.Spade && card.cardColor == currentCardOnTable.cardColor))) ||
                               (card.cardType == Card.CardType.Queen &&
                                (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade));

                    case 3: 
                        Debug.Log("Trzy");
                        return card.value == 3 ||
                               card.value == 2 && card.cardColor == currentCardOnTable.cardColor ||
                               (card.cardType == Card.CardType.King &&
                                ((card.cardColor == Card.CardColor.Heart && card.cardColor == currentCardOnTable.cardColor) ||
                                (card.cardColor == Card.CardColor.Spade && card.cardColor == currentCardOnTable.cardColor))) ||
                               (card.cardType == Card.CardType.Queen &&
                                (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade));

                    case 4: 
                        Debug.Log("Cztery");
                        return card.value == 4;

                    case 5: 
                        Debug.Log("Piec");
                        return true;

                    case 11:
                        return card.cardType == Card.CardType.Jack ||
                        card.value == requestedValue;

                    case 13:
                        if(currentCardOnTable.cardColor == Card.CardColor.Heart || currentCardOnTable.cardColor == Card.CardColor.Spade)
                        {
                            return (card.value == 2 && card.cardColor == currentCardOnTable.cardColor) ||
                                   (card.value == 3 && card.cardColor == currentCardOnTable.cardColor) ||
                                   (card.cardType == Card.CardType.King &&
                                    (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade)) ||
                                   (card.cardType == Card.CardType.Queen &&
                                    (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade));
                        }

                        else
                        {
                            return card.value == 13 || card.cardColor == currentCardOnTable.cardColor || card.value == 5;
                        }

                    case 14:
                        if(card.cardColor.ToString() == requestedColor || card.cardType == CardType.Ace || card.value == 5)
                        {
                            return true;
                        }

                        else
                        {
                            Debug.Log(requestedColor);
                            return false;
                        }

                    default:
                        if (card.cardColor.ToString() == currentSuit || card.value == currentCardOnTable.value || card.value == 5)
                        {
                            return true;
                        }
                        break;
                }
            }
        }
        
        return false;
    }

    public void HandleRequest(string playerName)
    {
        if(CanRespondToSpecialCard(playerName))
        {
            photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, true);
            ShowInfo("You can play!");
        }

        else
        {
            photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, false);
            if (request == Request.Draw)
            {
                Debug.Log("Cards to draw: " + cardsToDraw);
                for (int i = 0; i < cardsToDraw; i++)
                {
                    if (deck.Cards.Count == 0)
                    {
                        deck.CreateDeck();
                    }
                    Card drawnCard = deck.DrawCard();
                    photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                }

                ShowInfo("You can't respond!");
                photonView.RPC("SetCardsToDraw", RpcTarget.AllBuffered, 0);
                photonView.RPC("SetFirstCard", RpcTarget.AllBuffered);
                currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
                photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
            }

            if(request == Request.Stand)
            {
                ShowInfo("You can't respond!");
                currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
                photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
                photonView.RPC("SetFirstCard", RpcTarget.AllBuffered);
            }

            if(request == Request.Demand || request == Request.ChangeColor)
            {
                ShowInfo("You can't respond!");
                photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
            }
            
            else
            {
                //ShowInfo("You can play!");
                photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, true);
            }
        }
    }

    [PunRPC]
    public void SetFirstCard()
    {
        firstCard = true;
    }

    [PunRPC]
    public void SetCanHandleRequest(bool canHandle)
    {
        canHandleRequest = canHandle;
    }

    [PunRPC]
    public void SetCardsToDraw(int cardsCount)
    {
        cardsToDraw = cardsCount;
    }

    

    private bool CanRespondToSpecialCard(string playerId)
    {
        Player player = playersList[playerId];

        foreach (var card in player.playerCards)
        {
            if (IsCardPlayable(card))
            {
                return true;
            }
        }
        return false;
    }

}
