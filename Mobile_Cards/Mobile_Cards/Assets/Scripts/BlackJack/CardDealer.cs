using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

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

    private GameObject hiddenCardGameObject;

    public IEnumerator DealCards(Player player, int id, bool showCard)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, 0), canvasTransform);
        player.cardsObjects.Add(card);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
            Destroy(card);
            yield break;
        }

        cardImage.sprite = cardBackSprite;

        Card newCard = deck.GetCardById(id);
        if (newCard == null)
        {
            Debug.LogError($"Card with ID {id} is null.");
            Destroy(card);
            yield break;
        }

        Vector3 basePosition = player.position;
        int cardIndex = player.playerCards.Count;
        Vector3 targetPosition = basePosition + new Vector3(cardIndex * 60, 0, 0);


        yield return card.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();

        if (showCard)
        {
            cardImage.sprite = cardsModels.getSpriteId(id);
        }

        Debug.Log("Animacja zakończona!");
    }

    public void Deal(Player player, int id, bool showCard)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, 0), canvasTransform);
        player.cardsObjects.Add(card);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
            Destroy(card);
        }

        cardImage.sprite = cardBackSprite;

        Card newCard = deck.GetCardById(id);
        if (newCard == null)
        {
            Debug.LogError($"Card with ID {id} is null.");
            Destroy(card);
        }

        Vector3 basePosition = player.position;
        int cardIndex = player.playerCards.Count;
        Vector3 targetPosition = basePosition + new Vector3(cardIndex * 60, 0, 0);


        card.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).WaitForCompletion();

        if (showCard)
        {
            cardImage.sprite = cardsModels.getSpriteId(id);
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

    public IEnumerator PokerDealer(int cardId, Vector3 position, int id)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, 0), canvasTransform);

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

}
