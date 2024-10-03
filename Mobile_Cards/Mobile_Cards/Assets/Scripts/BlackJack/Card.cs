using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public CardType cardType;
    public CardColor cardColor;
    public int value;
    public int id;

    public Card(int id, int value, CardType cardType, CardColor cardColor)
    {
        this.cardType = cardType;
        this.cardColor = cardColor;
        this.value = value;
        this.id = id;
    }

    public enum CardType
    {
        None,
        Numerical,
        Jack,
        Queen,
        King,
        Ace
    }

    public enum CardColor
    {
        Spade,
        Hearts,
        Diamond,
        Club
    }

    public int getValue()
    {
        return value;
    }


}
