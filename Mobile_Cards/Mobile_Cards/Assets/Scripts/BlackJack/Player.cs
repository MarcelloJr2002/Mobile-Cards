using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public int score;
    public List<Card> playerCards;
    public double money;
    public int id;
    public double bet;

    public Transform position;
    public string playerName;
    public PhotonView photonView;
    public Photon.Realtime.Player photonPlayer;
    public bool aceInHand;

    private void Start()
    {
        if(photonView.IsMine)
        {
            photonPlayer = PhotonNetwork.LocalPlayer;
            id = photonPlayer.ActorNumber;
            playerName = photonPlayer.NickName;
        }
        
        score = 0;
        money = 1000;
        playerCards = new List<Card>();
        position.position = new Vector3(0, 0);
        aceInHand = false;
        bet = 0;
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

 

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerName);
            stream.SendNext(id);
            stream.SendNext(score);
        }
        else
        {
            playerName = (string)stream.ReceiveNext();
            id = (int)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
        }
    }
}
