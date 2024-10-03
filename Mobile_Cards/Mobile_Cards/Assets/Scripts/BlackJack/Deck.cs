using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    

    public void CreateDeck()
    {
        int id = 0;

        for (int j = 0; j < 13; j++)
        {
            if (j == 0) Cards.Add(new Card(id, 11, Card.CardType.Ace, Card.CardColor.Spade));
            else if (j == 10) Cards.Add(new Card(id, 10, Card.CardType.Jack, Card.CardColor.Spade));
            else if (j == 11) Cards.Add(new Card(id, 10, Card.CardType.Queen, Card.CardColor.Spade));
            else if (j == 12) Cards.Add(new Card(id, 10, Card.CardType.King, Card.CardColor.Spade));
            else Cards.Add(new Card(id, j + 1, Card.CardType.Numerical, Card.CardColor.Spade));
            id++;
        }

        for (int j = 0; j < 13; j++)
        {
            if (j == 0) Cards.Add(new Card(id, 11, Card.CardType.Ace, Card.CardColor.Club));
            else if (j == 10) Cards.Add(new Card(id, 10, Card.CardType.Jack, Card.CardColor.Club));
            else if (j == 11) Cards.Add(new Card(id, 10, Card.CardType.Queen, Card.CardColor.Club));
            else if (j == 12) Cards.Add(new Card(id, 10, Card.CardType.King, Card.CardColor.Club));
            else Cards.Add(new Card(id, j + 1, Card.CardType.Numerical, Card.CardColor.Club));
            id++;
        }

        for (int j = 0; j < 13; j++)
        {
            if (j == 0) Cards.Add(new Card(id, 11, Card.CardType.Ace, Card.CardColor.Hearts));
            else if (j == 10) Cards.Add(new Card(id, 10, Card.CardType.Jack, Card.CardColor.Hearts));
            else if (j == 11) Cards.Add(new Card(id, 10, Card.CardType.Queen, Card.CardColor.Hearts));
            else if (j == 12) Cards.Add(new Card(id, 10, Card.CardType.King, Card.CardColor.Hearts));
            else Cards.Add(new Card(id, j + 1, Card.CardType.Numerical, Card.CardColor.Hearts));
            id++;
        }

        for (int j = 0; j <= 13; j++)
        {
            if (j == 0) Cards.Add(new Card(id, 11, Card.CardType.Ace, Card.CardColor.Diamond));
            else if (j == 10) Cards.Add(new Card(id, 10, Card.CardType.Jack, Card.CardColor.Diamond));
            else if (j == 11) Cards.Add(new Card(id, 10, Card.CardType.Queen, Card.CardColor.Diamond));
            else if (j == 12) Cards.Add(new Card(id, 10, Card.CardType.King, Card.CardColor.Diamond));
            else Cards.Add(new Card(id, j + 1, Card.CardType.Numerical, Card.CardColor.Diamond));
            id++;
        }
    }

    public Card DrawCard()
    {
        if(Cards.Count == 0)CreateDeck();

        int id = Random.Range(0, Cards.Count);
        Card drawnCard = Cards[id];

        if(drawnCard != null)
        {
            Cards.RemoveAt(id);
            return drawnCard;
        }

        else
        {
            return DrawCard();
        }
    }


}
