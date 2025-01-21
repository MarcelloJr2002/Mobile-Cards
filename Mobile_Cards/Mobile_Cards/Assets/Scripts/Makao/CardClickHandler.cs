using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Card cardData;
    private Makao makaoGame;

    public void Initialize(Makao game)
    {
        makaoGame = game;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (makaoGame != null)
        {
            makaoGame.OnCardClicked(gameObject);
        }
    }
}
