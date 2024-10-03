using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTurn : MonoBehaviour
{
    public Transform position1;
    public Transform position2;
    public Transform position3;
    public Transform position4;
    public Transform hitPosition;
    public Transform standPosition;
    public Deck deck;
    public Player player;
    public Player dealer;
    public CardsModels cardModels;
    public GameObject faceDownCard1;
    public GameObject faceDownCard2;
    public GameObject faceDownCard3;
    public GameObject faceDownCard4;
    public Stats GameStats;
    public int betValue = Globals.betValue;
    private float movePosition = 0.15f;
    public ResultManager resultManager;


    private void Start()
    {
        player.money = Globals.playerMoney;
        //GameStats.DisplayMoney(player.money);
    }

    public void ChangeMoney()
    {
        player.money -= Globals.betValue;
        Globals.playerMoney -= Globals.betValue;
        //GameStats.DisplayMoney(player.money);
    }

    /*public void givePoints()
    {
        GameStats.DisplayScore(player.Score);
        GameStats.DisplayDealeScore(dealer.Score);
    }*/

    public void ShowCard1()
    {
        faceDownCard1.SetActive(false);
        Card drawnCard = deck.DrawCard();
        player.PlayerCards.Add(drawnCard);
        player.Score += drawnCard.getValue();
        //GameObject cardPrefab = cardModels.getCardPrefab(drawnCard.id);
        //GameObject firstcard = Instantiate(cardPrefab, position1.position, Quaternion.identity, position1);
        position1.position -= new Vector3(movePosition, 0f, 0f);
        //firstcard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
    }

    public void ShowCard2()
    {
        faceDownCard2.SetActive(false);
        Card drawnCard = deck.DrawCard();
        player.PlayerCards.Add(drawnCard);
        player.Score += drawnCard.getValue();
        //GameObject cardPrefub = cardModels.getCardPrefab(drawnCard.id);
        //GameObject secondCard = Instantiate(cardPrefub, position2.position, Quaternion.identity, position2);
        position2.position -= new Vector3(movePosition, 0f, 0f);
        //secondCard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        //givePoints();
    }


    public void ShowDealerCard()
    {
        faceDownCard3.SetActive(false);
        Card drawnCard = deck.DrawCard();
        dealer.PlayerCards.Add(drawnCard);
        dealer.Score += drawnCard.getValue();
        //GameObject cardPrefub = cardModels.getCardPrefab(drawnCard.id);
        //GameObject secondCard = Instantiate(cardPrefub, position3.position, Quaternion.identity, position3);
        position3.position -= new Vector3(movePosition, 0f, 0f);
        position4.position -= new Vector3(movePosition, 0f, 0f);
        //secondCard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
    }

    public void ShowSecondDealerCard()
    {
        faceDownCard4.SetActive(false);
        Card drawnCard = deck.DrawCard();
        dealer.PlayerCards.Add(drawnCard);
        dealer.Score += drawnCard.getValue();
        //GameObject cardPrefub = cardModels.getCardPrefab(drawnCard.id);
        //GameObject secondCard = Instantiate(cardPrefub, position4.position, Quaternion.identity, position4);
        //secondCard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        //givePoints();
    }

    public void Hit()
    {
        if (player.Score <= 21)
        {
            movePosition = 0f;
            Card drawnCard = deck.DrawCard();
            player.PlayerCards.Add(drawnCard);
            player.Score += drawnCard.getValue();
            //GameObject cardPrefab = cardModels.getCardPrefab(drawnCard.id);
            //GameObject hitCard = Instantiate(cardPrefab, hitPosition.position, Quaternion.identity, hitPosition);
            //hitCard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            hitPosition.position += new Vector3(movePosition, 0f, 0f);
            movePosition += 0.15f;
            //givePoints();
        }

        if (player.Score > 21)
        {
            //resultManager.DisplayLoseResult();
        }
    }

    public void Stand()
    {
        ShowSecondDealerCard();
        movePosition = 0f;
        Debug.Log(dealer.Score);
        Card drawnCard = deck.DrawCard();

        while (dealer.Score <= 17)
        {
            drawnCard = deck.DrawCard();
            dealer.PlayerCards.Add(drawnCard);
            dealer.Score += drawnCard.getValue();
            //GameObject cardPrefab = cardModels.getCardPrefab(drawnCard.id);
            //GameObject standCard = Instantiate(cardPrefab, standPosition.position, Quaternion.identity, standPosition);
            //standCard.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            standPosition.position += new Vector3(movePosition, 0f, 0f);
            movePosition += 0.15f;
            Debug.Log("Stand");
            //givePoints();
        }

        if (dealer.Score <= 21 && dealer.Score > player.Score)
        {
            //resultManager.DisplayLoseResult();
        }

        if (dealer.Score > 21)
        {
            Globals.playerMoney += Globals.betValue * 2;
            resultManager.DisplayWinResult();
        }

        if (player.Score > dealer.Score)
        {
            Globals.playerMoney += Globals.betValue * 2;
            resultManager.DisplayWinResult();
        }

        if (dealer.Score == player.Score)
        {
            Globals.playerMoney += Globals.betValue;
            resultManager.DisplayDrawResult();
        }

    }

    public void ResetGame()
    {
        player.Score = 0;
        dealer.Score = 0;
        player.PlayerCards.Clear();
        dealer.PlayerCards.Clear();
        resultManager.PlayAgain();
    }

    public void EndGame()
    {
        player.Score = 0;
        dealer.Score = 0;
        player.PlayerCards.Clear();
        dealer.PlayerCards.Clear();
        resultManager.BackToMenu();
    }

}
