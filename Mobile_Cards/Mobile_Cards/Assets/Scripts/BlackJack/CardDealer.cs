using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using static Makao;

public class CardDealer : MonoBehaviour
{
    public GameObject cardPrefab;         
    public Transform dealerPosition;      
    public Transform canvasTransform;
    public float speed = 500f;              
    public float cardOffsetY = 40f;
    public CardsModels cardsModels;       
    public Sprite cardBackSprite;         
    public Deck deck;
    public Text btText;
    public Button[] positions;
    public Transform cardsContainer1;
    public Transform cardsContainer2;
    public Transform tableContainer;
    public Button positionBtn1;
    public Button positionBtn2;

    private GameObject hiddenCardGameObject;


    public IEnumerator DealCards(Player player, int id, bool showCard)
    {
        Transform targetContainer = (player.position == positionBtn1.transform.position) ? cardsContainer1 : cardsContainer2;

        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.identity, targetContainer);
        player.cardsObjects.Add(card);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
            Destroy(card);
            yield break;
        }

        cardImage.sprite = cardBackSprite;
        int shift = 1;

        Card newCard = deck.GetCardById(id);
        if (newCard == null)
        {
            Debug.LogError($"Card with ID {id} is null.");
            Destroy(card);
            yield break;
        }

        int cardIndex = player.playerCards.Count;
        //card.GetComponent<Canvas>().sortingOrder = cardIndex;

        Vector3 basePosition = player.position;

        if (GameModeManager.Instance.selectedGameType == GameModeManager.GameType.Makao)
        {
            CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
            if (cardDisplay == null)
            {
                Debug.LogError("CardDisplay component missing on instantiated card!");
                yield break;
            }

            cardDisplay.SetCard(newCard);

            if(player.name == Globals.localPlayerId)
            {
                AddCardClickHandler(card, newCard);
            }
            cardIndex *= 3;
            
            if(player.position == positions[1].transform.position)
            {
                shift = -1;
                basePosition = player.position + new Vector3(shift * 320, 0, 0);
            }

            else if(player.position == positions[0].transform.position)
            {
                basePosition = player.position + new Vector3(-500, 0, 0);
            }

        }

        Vector3 targetPosition = basePosition + new Vector3(cardIndex * 60, 0, 0);

        yield return card.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();


        if (showCard)
        {
            cardImage.sprite = cardsModels.getSpriteId(id);
        }

        if (player.playerCards.Count > 6)
        {
            card.SetActive(false);
        }

        Debug.Log("Animacja zakończona!");
    }

    public void RenderHiddenCard(int id, GameObject hiddenCard)
    {
        Image cardImage = hiddenCard.GetComponent<Image>();

        hiddenCard.transform.DOScaleX(0, 0.3f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {

            // Zmień grafikę na odkrytą kartę
            cardImage.sprite = cardsModels.getSpriteId(id);

            // Przywróć skalę X z animacją
            hiddenCard.transform.DOScaleX(1, 0.3f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                Debug.Log("Second card revealed!");
            });
        });
    }

    public IEnumerator DestroyCards(GameObject playerCard)
    {

        Vector3 targetPosition = dealerPosition.position;
        Image cardImage = playerCard.GetComponent<Image>();
        cardImage.sprite = cardBackSprite;

        yield return playerCard.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();

    }

    public IEnumerator PokerDealer(int cardId, Vector3 position, int id, List<GameObject> cards)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, 0), tableContainer);
        cards.Add(card);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
            Destroy(card);
            yield break;
        }

        cardImage.sprite = cardBackSprite;

        Card newCard = deck.GetCardById(cardId);
        if (newCard == null)
        {
            Debug.LogError($"Card with ID {cardId} is null.");
            Destroy(card);
            yield break;
        }

        Vector3 basePosition = position;
        int cardIndex = id;

        Vector3 targetPosition = basePosition + new Vector3(cardIndex * 180, 0, 0);



        yield return card.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();

        cardImage.sprite = cardsModels.getSpriteId(cardId);

        Debug.Log("Animacja zakończona!");
    }

    public IEnumerator MakaoDealer(GameObject selectedCard, Vector3 position,List<GameObject> cards)
    {
        cards.Add(selectedCard);
        selectedCard.transform.SetParent(tableContainer);

        Vector3 targetPosition = position + new Vector3(180, 0, 0);
        yield return selectedCard.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();

        Debug.Log("Animacja zakończona!");
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

    private void AddCardClickHandler(GameObject cardObject, Card cardData)
    {
        var clickHandler = cardObject.AddComponent<CardClickHandler>();
        clickHandler.cardData = cardData;

        Makao makaoGame = FindObjectOfType<Makao>();
        if (makaoGame == null)
        {
            Debug.LogError("MakaoGame instance not found in scene!");
            return;
        }

        clickHandler.Initialize(makaoGame);
    }

    public void RearrangeCards(Player player, Transform parentTransform, float cardSpacing = 60f)
    {
        if (player == null || player.cardsObjects == null || player.cardsObjects.Count == 0)
        {
            Debug.LogWarning("Player has no cards to rearrange.");
            return;
        }

        // Obliczanie punktu początkowego, aby karty były wyśrodkowane
        float totalWidth = (player.cardsObjects.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        // Iteracja przez karty gracza
        for (int i = 0; i < player.cardsObjects.Count; i++)
        {
            GameObject cardObject = player.cardsObjects[i];
            Vector3 targetPosition = new Vector3(startX + i * cardSpacing, 0, 0);

            // Przesuwanie kart do nowych pozycji
            cardObject.transform.SetParent(parentTransform); // Ustawienie rodzica dla transformacji
            cardObject.transform.DOLocalMove(targetPosition, 0.5f).SetEase(Ease.OutCubic);
        }
    }



}
