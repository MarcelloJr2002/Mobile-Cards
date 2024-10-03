using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDealer : MonoBehaviour
{
    public GameObject cardPrefab;         // Prefab karty
    public Transform[] playerPositions;   // Pozycje graczy
    public Transform dealerPosition;      // Pozycja dealera
    public Transform canvasTransform;     // Transform Canvasu
    public float speed = 5f;              // Prędkość ruchu kart
    public int numberOfPlayers;           // Liczba aktywnych graczy
    public int cardsPerPlayer;            // Liczba kart na gracza
    public float cardOffsetY = 30f;       // Odstęp między kartami (na osi Y)
    public CardsModels cardsModels;       // Odniesienie do skryptu CardsModels, aby uzyskać Sprite karty
    public Sprite cardBackSprite;         // Sprite odwrotnej strony karty

    private void Start()
    {
        StartCoroutine(DealCards());
    }

    // Metoda rozdająca karty graczom
    public IEnumerator DealCards()
    {
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            for (int player = 0; player < numberOfPlayers; player++)
            {
                // Tworzymy kartę jako dziecko Canvasu
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

                // Obliczamy pozycję docelową karty (dodajemy przesunięcie na osi Y)
                Vector3 targetPosition = playerPositions[player].position;
                targetPosition.y -= cardOffsetY * i; // Każda kolejna karta gracza jest przesunięta na osi Y

                // Przesuwamy kartę do pozycji gracza
                StartCoroutine(MoveCard(card, targetPosition));

                // Czekamy chwilę, zanim przejdziemy do kolejnego gracza
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // Metoda odpowiedzialna za animację ruchu karty
    private IEnumerator MoveCard(GameObject card, Vector3 targetPosition)
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
            int cardId = Random.Range(0, cardsModels.cardSprites.Count); // Losujemy ID karty
            cardImage.sprite = cardsModels.getSpriteId(cardId); // Przypisujemy obraz karty
        }
    }
}
