using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;

public class Player
{
    public int score;
    public List<Card> playerCards;
    public double money;
    public int id;
    public double bet;
    public string UserName;

    //public Transform position;
    public Vector2 position;
    public Photon.Realtime.Player PhotonPlayer;
    public bool aceInHand;

    public Player(Photon.Realtime.Player photonPlayer)
    {
        PhotonPlayer = photonPlayer;
        playerCards = new List<Card>();
        score = 0;
        aceInHand = false;
        bet = 0;
        money = 1000;
        position = new Vector2(0, 0);
    }


    public Player(string userName)
    {
        UserName = userName;
        score = 0; 
        aceInHand = false; 
        bet = 0;
        money = 1000;
        position = new Vector2(0, 0);
    }

    public Player()
    {
        playerCards = new List<Card>();
        score = 0;
        money= 1000;
        aceInHand = false;
        position = new Vector2(0, 0);
    }

    public void SetPosition(Vector2  newPosition)
    {
        
        position = newPosition;
    }

    public void AddCardToHand(Card card)
    {
        playerCards.Add(card);
        score += card.value;

        if (card.cardType == Card.CardType.Ace)
        {
            aceInHand = true;
        }
        if (aceInHand && score > 21)
        {
            score -= 10;
            aceInHand &= false;
        }
    }

    public bool IfBlackJack()
    {
        if(playerCards.Count == 2)
        {

            bool ifAce = false;
            bool if10ScoreCard = false;

            foreach (var card in playerCards)
            {
                if (card.cardType == Card.CardType.Ace)
                {
                    ifAce = true;
                }
                else if (card.value == 10)
                {
                    if10ScoreCard = true;
                }

                if (ifAce && if10ScoreCard)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool Busted()
    {
        if(score > 21)
        {
            return true;
        }
        return false;
    }

    

    
    public void ResetHand()
    {
        playerCards.Clear();
        score = 0;
    }

    
    public void SetMoney(int amount)
    {
        money = amount;
    }

    
    public void DeductMoney(double amount)
    {
        money -= amount;
    }

    
    public void AddMoney(double amount)
    {
        money += amount;
    }

    
    public int GetCardCount()
    {
        return playerCards.Count;
    }

    
    public int GetScore()
    {
        return score;
    }

    
    public double GetMoney()
    {
        return money;
    }
}
