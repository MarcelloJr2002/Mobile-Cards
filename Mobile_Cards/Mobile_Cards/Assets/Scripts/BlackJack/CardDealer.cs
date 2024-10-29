using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDealer : MonoBehaviour
{
    public GameObject cardPrefab;         
    public Transform dealerPosition;      
    public Transform canvasTransform;     
    public float speed = 5f;              
    public float cardOffsetY = 30f;       
    public CardsModels cardsModels;       
    public Sprite cardBackSprite;         
    public Deck deck;


    public IEnumerator DealCards(Player player)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, -90), canvasTransform);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = cardBackSprite; 
        }
        else
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
        }

        Vector3 targetPosition = player.position;

        StartCoroutine(MoveCard(card, targetPosition, player));

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator MoveCard(GameObject card, Vector3 targetPosition, Player player)
    {
        RectTransform cardRectTransform = card.GetComponent<RectTransform>();
        while (Vector3.Distance(cardRectTransform.position, targetPosition) > 0.01f)
        {
            cardRectTransform.position = Vector3.MoveTowards(cardRectTransform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        cardRectTransform.position = targetPosition;

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            Card drawnCard = deck.DrawCard();
            cardImage.sprite = cardsModels.getSpriteId(drawnCard.id);
            player.AddCardToHand(drawnCard);
        }
    }
}
