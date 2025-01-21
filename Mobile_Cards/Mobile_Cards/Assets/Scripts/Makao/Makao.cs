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
    private string requestedColor;
    private bool canHandleRequest = true;
    private int demandRequest = 0;
    private int colorRequest = 0;
    private bool ifRequested = false;

    public Dropdown extraCardsDropdown;
    public GameObject cardPrefab;
    public GameObject selectedCardHighlightPrefab;
    public GameObject cardHighlightPrefab;

    private List<GameObject> displayedCards = new List<GameObject>();
    private List<Card> selectedCards = new List<Card>();
    private List<GameObject> selectedCardsObjects = new List<GameObject>();
    private Dictionary<GameObject, GameObject> cardHighlights = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> requestHighlights = new Dictionary<GameObject, GameObject>();
    private List<Card> extraCards = new List<Card>();
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

    [PunRPC]
    public void AddPlayer(string playerName, Vector3 position)
    {
        Player newPlayer = new(PhotonNetwork.LocalPlayer);
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

    [PunRPC]
    public void StartGameRPC()
    {
        currentPlayerIndex = 0;
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
    }

    public override void GameStart()
    {
        base.GameStart();
        if(playersList.Count >= 1)
        {
            masterButton.SetActive(false);
            photonView.RPC("StartGameRPC", RpcTarget.AllBuffered);
            DealInitialCards();

            if (PhotonNetwork.IsMasterClient)
            {
                ConfirmButton.SetActive(true);
                extraCardsDropdown.gameObject.SetActive(true);
                drawCardButton.SetActive(true);
            }
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

        if (playersList[playerId].position == positionButtons[0].transform.position)
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer1);
        }

        else
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer2);
        }

        Debug.Log($"Player {pId} hand size: {playersList[pId].playerCards.Count}");
        Debug.Log($"Player {pId} object count: {playersList[pId].cardsObjects.Count}");
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

            Debug.Log("Flop card: " + card.value + " " + card.cardType + " " + card.cardColor);
            Debug.Log("Flop card: " + card.id);

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

        Debug.Log("Flop card rpc: " + card.value + " " + card.cardType + " " + card.cardColor);

        Debug.Log("Flop card rpc: " + card.id);

        StartCoroutine(cardDealer.PokerDealer(card.id, tablePosition.position, howMany, cardsOnTable));
    }

    [PunRPC]
    public void MoveCardFromHand(int cardId)
    {
        Card card = rpcDeck.GetCardById(cardId);
        tableCards.Add(card);
        currentCardOnTable = card;
        requestedCard = card;
        currentSuit = card.cardColor.ToString();
        playersList[playerId].playerCards.Remove(card);

        Debug.Log("Flop card rpc: " + card.value + " " + card.cardType + " " + card.cardColor);

        Debug.Log("Flop card rpc: " + card.id);

        foreach(var cardObject in selectedCardsObjects)
        {
            StartCoroutine(cardDealer.MakaoDealer(cardObject, tablePosition.position, cardsOnTable));
            playersList[playerId].cardsObjects.Remove(cardObject);
        }

        if (playersList[playerId].position == positionButtons[0].transform.position)
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer1);
        }

        else
        {
            cardDealer.RearrangeCards(playersList[playerId], cardsContainer2);
        }
    }

    [PunRPC]
    public void SetColor(string color)
    {
        requestedColor = color;
    }

    [PunRPC]
    public void SetRequestedCardRPC(int id)
    {
        Card card = rpcDeck.GetCardById(id);
        SetRequestedCard(card);
    }

    public void SetRequestedCard(Card card)
    {
        requestedCard = card;
    }


    public void OnCardClicked(GameObject cardObject)
    {
        playerId = playersList.Keys.ElementAt(currentPlayerIndex);
        Debug.Log($"OnCardClicked called with object: {cardObject.name}");

        if (playerId == Globals.localPlayerId)
        {
            Card card = cardObject.GetComponent<CardDisplay>().Card;

            SetRequest(card);

            if(request == Request.Demand && ifRequested)
            {
                if(card.value >= 6 && card.value <=10)
                {
                    if (requestHighlights.ContainsKey(cardObject))
                    {
                        Destroy(requestHighlights[cardObject]);
                        requestHighlights.Remove(cardObject);
                        selectedRequestCard.Remove(card);
                        demandRequest = 0;
                        ifRequested = true;
                        Debug.Log("Card deselected.");
                    }

                    if (selectedRequestCard.Count == 0)
                    {
                        requestedCard = card;
                        photonView.RPC("SetRequestedCard", RpcTarget.AllBuffered, card.id);
                        selectedRequestCard.Add(card);
                        requestedColor = card.cardColor.ToString();
                        photonView.RPC("SetColor", RpcTarget.AllBuffered, card.cardColor.ToString());
                        GameObject highlight = Instantiate(cardHighlightPrefab, canvasTransform);

                        highlight.transform.position = cardObject.transform.position + new Vector3(0, 200, 0);
                        highlight.SetActive(true);
                        requestHighlights[cardObject] = highlight;
                        demandRequest = 1;
                        //ifRequested = false;
                        ShowInfo("You chose: " + requestedCard.value + " of " + requestedColor);
                    }

                    else
                    {
                        ShowInfo("You can choose only one card!");
                    }
                }

                else
                {
                    ShowInfo("You can choose only cards from 6 to 10!");
                }
            }

            if(request == Request.ChangeColor && ifRequested)
            {
                if (requestHighlights.ContainsKey(cardObject))
                {
                    Destroy(requestHighlights[cardObject]);
                    requestHighlights.Remove(cardObject);
                    selectedRequestCard.Remove(card);
                    colorRequest = 0;
                    ifRequested = true;
                    Debug.Log("Card deselected.");
                }

                if (selectedRequestCard.Count == 0)
                {
                    requestedCard = card;
                    photonView.RPC("SetRequestedCard", RpcTarget.AllBuffered, card.id);
                    selectedRequestCard.Add(card);
                    requestedColor = card.cardColor.ToString();
                    photonView.RPC("SetColor", RpcTarget.AllBuffered, card.cardColor.ToString());
                    GameObject highlight = Instantiate(cardHighlightPrefab, canvasTransform);

                    highlight.transform.position = cardObject.transform.position + new Vector3(0, 200, 0);
                    highlight.SetActive(true);
                    requestHighlights[cardObject] = highlight;
                    colorRequest = 1;
                    //ifRequested = false;
                    ShowInfo("You chose: " + requestedCard.value + " of " + requestedColor);
                }

                else
                {
                    ShowInfo("You can choose only one card!");
                }
            }

            else
            {
                Debug.Log("Card value: " + card.value + " CardColor: " + card.cardColor);
                if (cardHighlights.ContainsKey(cardObject))
                {
                    Destroy(cardHighlights[cardObject]);
                    cardHighlights.Remove(cardObject);
                    selectedCards.Remove(card);
                    Debug.Log(selectedCards.Count);
                    Debug.Log("Card deselected.");
                }

                else if (IsCardPlayable(card))
                {
                    Debug.Log("Is playable!");

                    GameObject highlight = Instantiate(selectedCardHighlightPrefab, canvasTransform);

                    highlight.transform.position = cardObject.transform.position + new Vector3(0, 200, 0);
                    highlight.SetActive(true);

                    if (card.cardType == CardType.Jack || card.cardType == CardType.Ace)
                    {
                        ifRequested = true;
                    }

                    // Zapisanie podświetlenia w słowniku
                    cardHighlights[cardObject] = highlight;

                    // Dodanie karty do listy zaznaczonych
                    selectedCards.Add(card);
                    selectedCardsObjects.Add(cardObject);
                    Debug.Log("Card selected: " + card.value + card.cardColor);
                }
            }

            
        }
    }

    public void TakeCard()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            playerId = playersList.Keys.ElementAt(currentPlayerIndex);
            Card drawnCard = deck.DrawCard();

            photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);

            if (playerId == Globals.localPlayerId)
            {
                //UpdatePlayerCardsUI(playersList[playerId]);
            }
        }
        drawCardButton.SetActive(false);
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
            if(ifRequested)
            {
                ShowInfo("Choose card!");
            }

            if(demandRequest == 1)
            {
                ifRequested = false;
            }

            if(colorRequest == 1)
            {
                ifRequested = false;
            }
            

            else
            {
                if (demandRequest == 1 || colorRequest == 1)
                {
                    selectedRequestCard.Clear();

                    foreach (var highlight in requestHighlights.Values)
                    {
                        Destroy(highlight);
                    }
                    requestHighlights.Clear();
                    demandRequest = 0;
                    colorRequest = 0;
                }

                //extraCardsDropdown.gameObject.SetActive(false);
                ConfirmButton.SetActive(false);
                foreach (Card card in selectedCards)
                {
                    photonView.RPC("MoveCardFromHand", RpcTarget.AllBuffered, card.id);
                    Debug.Log($"Card {card.cardType} of {card.cardColor} added to the table.");
                }

                Player currentPlayer = playersList[Globals.localPlayerId];
                foreach (Card card in selectedCards)
                {
                    currentPlayer.playerCards.Remove(card);

                    if (extraCards.Contains(card))
                    {
                        extraCards.Remove(card);
                    }
                }

                selectedCards.Clear();
                foreach (var highlight in cardHighlights.Values)
                {
                    Destroy(highlight);
                }
                cardHighlights.Clear();
                selectedCards.Clear();

                if (currentPlayer.playerCards.Count == 0)
                {
                    EndGame();
                }

                else
                {
                    //extraCardsDropdown.ClearOptions();
                    //UpdatePlayerCardsUI(playersList[Globals.localPlayerId]);
                    NextTurn();
                }
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

    private void UpdatePlayerCardsUI(Player player)
    {
        // Usuń stare karty z widoku
        foreach (var cardObject in displayedCards)
        {
            Destroy(cardObject);
        }
        displayedCards.Clear();

        // Zresetuj menu rozwijane
        extraCardsDropdown.ClearOptions();
        extraCards.Clear();

        Transform container = (player.position == positionButtons[0].transform.position)
    ? cardsContainer1
    : cardsContainer2;

        for (int i = 0; i < Mathf.Min(player.playerCards.Count, maxVisibleCards); i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, container);
            cardObject.GetComponent<CardDisplay>().SetCard(player.playerCards[i]);
            displayedCards.Add(cardObject);
            player.cardsObjects.Add(cardObject);
        }


        // Dodaj nadmiarowe karty do dropdown (tylko tekst, nie prefab!)
        for (int i = maxVisibleCards; i < player.playerCards.Count; i++)
        {
            Card extraCard = player.playerCards[i];
            extraCards.Add(extraCard);

            // Upewnij się, że dodajesz tylko opcje do dropdown, a nie tworzysz obiektów
            extraCardsDropdown.options.Add(new Dropdown.OptionData($"{extraCard.cardType} {extraCard.cardColor}"));
        }


        // Pokaż menu dropdown, jeśli są dodatkowe karty
        //extraCardsDropdown.gameObject.SetActive(extraCards.Count > 0);
    }


    public void OnDropdownCardSelected(int index)
    {
        if (index < 0 || index >= extraCards.Count)
        {
            Debug.LogError("Invalid card index selected in dropdown.");
            return;
        }

        Card selectedCard = extraCards[index];
        Debug.Log($"Selected card from dropdown: {selectedCard.cardType} {selectedCard.cardColor}");

        // Dodaj wybraną kartę do listy zaznaczonych
        selectedCards.Add(selectedCard);

        // Opcjonalne podświetlenie karty w dropdown (do zaimplementowania, jeśli potrzebne)
    }



    public override void EndGame()
    {
        base.EndGame();
    }

    public void SetRequest(Card card)
    {
        switch(card.value)
        {
            case 2:
                request = Request.Draw;
                cardsToDraw += 2;
                break;
            case 3:
                request = Request.Draw;
                cardsToDraw += 3;
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
            if (card.value == currentCardOnTable.value)
            {
                Debug.Log("Selected card count > 0");
                return true;
            }

            else 
            {
                Debug.Log("Selected card count > 0");
                return false; 
            }
        }

        else
        {
            if (card.cardColor.ToString() == currentSuit || card.value == currentCardOnTable.value)
            {
                return true;
            }

            // Rozszerzenie reguł dla kart specjalnych
            if (currentCardOnTable != null)
            {
                switch (currentCardOnTable.value)
                {
                    case 2: // Dwójka
                        Debug.Log("Dwa");
                        return card.value == 2 ||
                               card.value == 3 && card.cardColor == currentCardOnTable.cardColor ||
                               (card.cardType == Card.CardType.King &&
                                ((card.cardColor == Card.CardColor.Heart && card.cardColor == currentCardOnTable.cardColor) ||
                                (card.cardColor == Card.CardColor.Spade && card.cardColor == currentCardOnTable.cardColor))) ||
                               (card.cardType == Card.CardType.Queen &&
                                (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade));

                    case 3: // Trójka
                        Debug.Log("Trzy");
                        return card.value == 3 ||
                               card.value == 2 && card.cardColor == currentCardOnTable.cardColor ||
                               (card.cardType == Card.CardType.King &&
                                ((card.cardColor == Card.CardColor.Heart && card.cardColor == currentCardOnTable.cardColor) ||
                                (card.cardColor == Card.CardColor.Spade && card.cardColor == currentCardOnTable.cardColor))) ||
                               (card.cardType == Card.CardType.Queen &&
                                (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade));

                    case 4: // Czwórka (blokada)
                        Debug.Log("Cztery");
                        return card.value == 4;

                    case 5: // Piątka na wszystko
                        Debug.Log("Piec");
                        return true;

                    case 11:
                        return card.cardType == Card.CardType.Jack ||
                        card.value == requestedCard.value;

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
                            return card.value == 13 || card.cardColor == currentCardOnTable.cardColor;
                        }

                    case 14:
                        return card.cardColor == requestedCard.cardColor;
                }
            }
        }
        
        // Sprawdzenie podstawowych reguł
        

        // Jeśli żadna z powyższych reguł nie została spełniona
        return false;
    }


    private void HandleSpecialCard(Card card, Player currentPlayer)
    {
        switch (card.value)
        {
            case 2:
                Debug.Log("Special card: 2 played! Next player must draw 2 cards.");
                photonView.RPC("HandleDrawCards", RpcTarget.AllBuffered, 2);
                break;

            case 3:
                Debug.Log("Special card: 3 played! Next player must draw 3 cards.");
                photonView.RPC("HandleDrawCards", RpcTarget.AllBuffered, 3);
                break;

            case 4:
                Debug.Log("Special card: 4 played! Next player loses their turn.");
                photonView.RPC("HandleSkipTurn", RpcTarget.AllBuffered);
                break;

            default:
                if (card.cardType == Card.CardType.Jack)
                {
                    Debug.Log("Special card: Jack played! Requesting a card.");
                    photonView.RPC("HandleRequestCard", RpcTarget.AllBuffered, currentPlayer.UserName, card.cardColor.ToString());
                }
                else if (card.cardType == Card.CardType.King && (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade))
                {
                    Debug.Log("Special card: King played! Next player must draw 5 cards.");
                    photonView.RPC("HandleDrawCards", RpcTarget.AllBuffered, 5);
                }
                else if (card.cardType == Card.CardType.Queen && (card.cardColor == Card.CardColor.Heart || card.cardColor == Card.CardColor.Spade))
                {
                    Debug.Log("Special card: Queen played! Blocks card draw effects.");
                    photonView.RPC("HandleBlockDrawCards", RpcTarget.AllBuffered);
                }
                else if (card.cardType == Card.CardType.Ace)
                {
                    Debug.Log("Special card: Ace played! Changing suit.");
                    photonView.RPC("HandleChangeSuit", RpcTarget.AllBuffered);
                }
                break;
        }
    }


    [PunRPC]
    public void HandleDrawCards(int cardsToDraw)
    {
        string nextPlayerId = nextId;
        if (playersList[nextPlayerId].playerCards.Count == 0)
        {
            Debug.Log("Next player has no cards left! Skipping draw.");
            NextTurn();
            return;
        }

        Debug.Log($"Next player {nextPlayerId} must draw {cardsToDraw} cards.");

        // Jeśli gracz ma karty, umożliw mu odpowiedź
        if (CanRespondToSpecialCard(nextPlayerId))
        {
            Debug.Log("Player can respond to special card. Waiting for response...");
            NextTurn();
        }
        else
        {
            Debug.Log("Player cannot respond. Automatically drawing cards.");
            //DrawCards(playersList[nextPlayerId], cardsToDraw);
            for(int i=0; i<cardsToDraw; i++)
            {
                TakeCard();
            }
            NextTurn();
        }
    }

    [PunRPC]
    public void HandleSkipTurn()
    {
        NextTurn();
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
                if(PhotonNetwork.IsMasterClient)
                {
                    for (int i = 0; i < cardsToDraw; i++)
                    {
                        Card drawnCard = deck.DrawCard();
                        if (IsCardPlayable(drawnCard))
                        {
                            ShowInfo("You can play!");
                            photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, true);
                            break;
                        }

                        else
                        {
                            photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                        }
                    }
                }
            }

            if(request == Request.Stand)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
                photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
            }

            if(request == Request.Demand || request == Request.ChangeColor)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    Card drawnCard = deck.DrawCard();
                    //photonView.RPC("DealCardToPlayer", RpcTarget.AllBuffered, playerId, drawnCard.id);
                    if(IsCardPlayable(drawnCard))
                    {
                        photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, true);
                        ShowInfo("You can play!");
                    }

                    else
                    {
                        currentPlayerIndex = (currentPlayerIndex + 1) % playersList.Count;
                        photonView.RPC("UpdateTurn", RpcTarget.AllBuffered, currentPlayerIndex);
                    }
                }
            }

            else
            {
                ShowInfo("You can play!");
                photonView.RPC("SetCanHandleRequest", RpcTarget.AllBuffered, true);
            }
        }
    }

    [PunRPC]
    public void SetCanHandleRequest(bool canHandle)
    {
        canHandleRequest = canHandle;
    }

    [PunRPC]
    public void HandleRequestCard(string requesterName, string requestedSuit)
    {
        Debug.Log($"Player {requesterName} requests suit {requestedSuit}.");
        currentSuit = requestedSuit;

        if (CanRespondToSpecialCard(nextId))
        {
            Debug.Log("Player can respond to special card. Waiting for response...");
            NextTurn();
        }
        else
        {
            Debug.Log("Player cannot respond. Automatically drawing cards.");
            //DrawCards(playersList[nextPlayerId], cardsToDraw);
            TakeCard();
        }

        NextTurn();
    }

    [PunRPC]
    public void HandleChangeSuit()
    {
        // Wyświetl UI dla gracza zmieniającego kolor 
        Debug.Log("Ace played! Prompting for suit change.");
        //ShowSuitChangeUI(); - do zrobienia jeszcze
    }

    [PunRPC]
    public void HandleBlockDrawCards()
    {
        Debug.Log("Queen played! Blocking card draw effects.");
        cardsToDraw = 0;
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
