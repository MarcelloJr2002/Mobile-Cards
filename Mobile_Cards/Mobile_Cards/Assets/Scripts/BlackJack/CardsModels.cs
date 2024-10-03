using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardsModels : MonoBehaviour
{
    public List<Sprite> cardSprites = new List<Sprite>();

    public Sprite getSpriteId(int id)
    {
        if (id >= 0 && id <cardSprites.Count)
        {
            return cardSprites[id];
        }
        else
        {
            Debug.Log("Id out of range!");
            return null;
        }
    }
}
