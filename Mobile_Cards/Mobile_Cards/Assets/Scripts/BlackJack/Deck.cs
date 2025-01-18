using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    //public CardsModels sprites;

    private void Start()
    {
        CreateDeck();
    }


    public void CreateDeck()
    {
        int id = 0;

        // Pobieramy aktualny typ gry
        string gameType = GameModeManager.Instance.selectedGameType.ToSafeString();

        Debug.Log("Game type: " + gameType);

        // Przechodzimy przez wszystkie kolory kart
        foreach (Card.CardColor color in System.Enum.GetValues(typeof(Card.CardColor)))
        {
            for (int j = 0; j < 13; j++)
            {
                int cardValue;
                Card.CardType cardType;

                // Przypisujemy wartości i typy kart na podstawie gameType
                if (j == 0)
                {
                    cardValue = gameType == "BlackJack" ? 11 : 14;
                    cardType = Card.CardType.Ace;
                }
                else if (j == 10)
                {
                    cardValue = gameType == "BlackJack" ? 10 : 11;
                    cardType = Card.CardType.Jack;
                }
                else if (j == 11)
                {
                    cardValue = gameType == "BlackJack" ? 10 : 12;
                    cardType = Card.CardType.Queen;
                }
                else if (j == 12)
                {
                    cardValue = gameType == "BlackJack" ? 10 : 13;
                    cardType = Card.CardType.King;
                }
                else
                {
                    cardValue = j + 1; // Wartość numeryczna
                    cardType = Card.CardType.Numerical;
                }

                // Tworzymy i dodajemy kartę do talii
                Cards.Add(new Card(id, cardValue, cardType, color));
                id++;
            }
        }
    }


    public Card DrawCard()
    {
        if (Cards == null || Cards.Count == 0)
        {
            CreateDeck();
        }

        int id = Random.Range(0, Cards.Count); // Poprawny zakres
        Card drawnCard = Cards[id];

        //Cards.RemoveAt(id);
        return drawnCard;
    }


    public Card GetCardById(int id)
    {
        if (Cards == null || Cards.Count == 0)
        {
            Debug.LogError("No cards in the deck. Returning null.");
            return null;
        }

        if (id < 0 || id >= Cards.Count)
        {
            Debug.LogError($"Invalid card ID: {id}. Returning null.");
            return null;
        }

        Card card = Cards[id];
        //Cards.RemoveAt(id);
        return card;
    }



}
