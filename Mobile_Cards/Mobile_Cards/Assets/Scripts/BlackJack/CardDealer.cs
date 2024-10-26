using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDealer : MonoBehaviour
{
    public GameObject cardPrefab;         // Prefab karty
    public Transform dealerPosition;      // Pozycja dealera
    public Transform canvasTransform;     // Transform Canvasu
    public float speed = 5f;              // Prędkość ruchu kart
    public float cardOffsetY = 30f;       // Odstęp między kartami (na osi Y)
    public CardsModels cardsModels;       // Odniesienie do skryptu CardsModels, aby uzyskać Sprite karty
    public Sprite cardBackSprite;         // Sprite odwrotnej strony karty
    public Deck deck;

    private int numberOfPlayers; // Teraz prywatna zmienna

    // Metoda do ustawiania liczby graczy
    public void SetNumberOfPlayers(int num)
    {
        numberOfPlayers = num;
    }

    // Metoda rozdająca karty graczom z parametrami
    public IEnumerator DealCards(Player player)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, -90), canvasTransform);

        // Ustawiamy obraz odwrotnej strony karty
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = cardBackSprite; // Ustawiamy obraz odwrotnej strony karty
        }
        else
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
        }

        // Pozycja docelowa karty dla tego konkretnego gracza
        Vector3 targetPosition = player.position.position;

        // Przesuwamy kartę do pozycji gracza
        StartCoroutine(MoveCard(card, targetPosition, player));

        // Czekamy chwilę, zanim zakończymy
        yield return new WaitForSeconds(0.5f);
    }

    // Metoda odpowiedzialna za animację ruchu karty
    private IEnumerator MoveCard(GameObject card, Vector3 targetPosition, Player player)
    {
        RectTransform cardRectTransform = card.GetComponent<RectTransform>();
        while (Vector3.Distance(cardRectTransform.position, targetPosition) > 0.01f)
        {
            cardRectTransform.position = Vector3.MoveTowards(cardRectTransform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Ustawiamy kartę dokładnie w miejscu docelowym
        cardRectTransform.position = targetPosition;

        // Po dotarciu do celu zmieniamy obraz na właściwy
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            Card drawnCard = deck.DrawCard();
            cardImage.sprite = cardsModels.getSpriteId(drawnCard.id);
            player.AddCardToHand(drawnCard);
        }
    }
}
