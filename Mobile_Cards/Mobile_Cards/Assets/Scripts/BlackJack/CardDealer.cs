using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;

public class CardDealer : MonoBehaviour
{
    public GameObject cardPrefab;         
    public Transform dealerPosition;      
    public Transform canvasTransform;     
    public float speed = 500f;              
    public float cardOffsetY = 40f;       
    public CardsModels cardsModels;       
    public Sprite cardBackSprite;         
    //public Deck deck;

    public void DealCards(Player player, int id)
    {
        GameObject card = Instantiate(cardPrefab, dealerPosition.position, Quaternion.Euler(0, 0, 0), canvasTransform);

        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = cardBackSprite;
        }
        else
        {
            Debug.LogError("Prefab karty nie ma komponentu Image.");
        }


        int cardId = id;
        Vector3 basePosition = player.position;
        int cardIndex = player.playerCards.Count - 1;
        Vector3 targetPosition = basePosition + new Vector3(cardIndex * 60, 0, 0);

        card.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutCubic).OnComplete(() =>
        {

            Image cardImage = card.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = cardsModels.getSpriteId(id);
            }
            Debug.Log("Animacja zakończona!");
        });


    }
}
