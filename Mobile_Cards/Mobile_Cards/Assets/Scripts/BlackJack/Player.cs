using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Score;
    public List<Card> PlayerCards;
    public int money;

    private void Start()
    {
        this.Score = 0;
        money = 0;
        PlayerCards = new List<Card>();
    }

    public int GetScore()
    {
        return this.Score;
    }

    public int GetMoney()
    {
        return money;
    }

    public void AddCardToHand(Card card)
    {
        PlayerCards.Add(card);
        Score += card.value;
    }

    public void ResetHand()
    {
        PlayerCards.Clear();
        Score = 0;
    }
}
