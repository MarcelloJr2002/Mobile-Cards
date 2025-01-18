using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardCombinatorics
{
    public static List<List<Card>> GetAllCombinations(List<Card> cards, int combinationSize = 5)
    {
        var result = new List<List<Card>>();
        GenerateCombinations(cards, new List<Card>(), 0, combinationSize, result);
        return result;
    }

    private static void GenerateCombinations(List<Card> cards, List<Card> current, int start, int size, List<List<Card>> result)
    {
        if (current.Count == size)
        {
            result.Add(new List<Card>(current));
            return;
        }

        for (int i = start; i < cards.Count; i++)
        {
            if (!current.Contains(cards[i]))
            {
                current.Add(cards[i]);
                GenerateCombinations(cards, current, i + 1, size, result);
                current.RemoveAt(current.Count - 1);
            }
        }
    }
}
