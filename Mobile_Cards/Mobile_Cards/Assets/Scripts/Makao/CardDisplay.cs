using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    public Card Card { get; private set; }

    public void SetCard(Card card)
    {
        Card = card;
    }
}
