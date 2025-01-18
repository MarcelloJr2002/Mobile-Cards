using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokerHandEvaluator : MonoBehaviour
{
    public enum HandRank
    {
        HighCard = 1,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }

    public class HandResult
    {
        public HandRank Rank;
        public List<int> TiebreakerValues; // Używane do porównania przy remisach
        public string Description;

        public HandResult(HandRank rank, List<int> tiebreakerValues, string description)
        {
            Rank = rank;
            TiebreakerValues = tiebreakerValues;
            Description = description;
        }
    }

    // Metoda główna do oceny rąk pokerowych
    public static HandResult EvaluateHand(List<Card> cards)
    {
        if (cards == null || cards.Count != 5)
            throw new System.Exception("Hand must contain exactly 5 cards.");

        cards = cards.OrderByDescending(c => c.value).ToList();

        bool isFlush = IsFlush(cards);
        bool isStraight = IsStraight(cards);
        var cardGroups = cards.GroupBy(c => c.value).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();

        if (isFlush && isStraight)
        {
            if (cards[0].value == 14) // Ace-high
                return new HandResult(HandRank.RoyalFlush, new List<int>(), "Royal Flush");
            return new HandResult(HandRank.StraightFlush, new List<int> { cards[0].value }, "Straight Flush");
        }
        if (cardGroups[0].Count() == 4)
            return new HandResult(HandRank.FourOfAKind, new List<int> { cardGroups[0].Key }, "Four of a Kind");
        if (cardGroups[0].Count() == 3 && cardGroups[1].Count() == 2)
            return new HandResult(HandRank.FullHouse, new List<int> { cardGroups[0].Key, cardGroups[1].Key }, "Full House");
        if (isFlush)
            return new HandResult(HandRank.Flush, cards.Select(c => c.value).ToList(), "Flush");
        if (isStraight)
            return new HandResult(HandRank.Straight, new List<int> { cards[0].value }, "Straight");
        if (cardGroups[0].Count() == 3)
            return new HandResult(HandRank.ThreeOfAKind, new List<int> { cardGroups[0].Key }, "Three of a Kind");
        if (cardGroups[0].Count() == 2 && cardGroups[1].Count() == 2)
            return new HandResult(HandRank.TwoPair, new List<int> { cardGroups[0].Key, cardGroups[1].Key }, "Two Pair");
        if (cardGroups[0].Count() == 2)
            return new HandResult(HandRank.OnePair, new List<int> { cardGroups[0].Key }, "One Pair");

        return new HandResult(HandRank.HighCard, cards.Select(c => c.value).ToList(), "High Card");
    }

    private static bool IsFlush(List<Card> cards)
    {
        return cards.All(c => c.cardColor == cards[0].cardColor);
    }

    private static bool IsStraight(List<Card> cards)
    {
        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (cards[i].value - cards[i + 1].value != 1)
            {
                if (i == 0 && cards[i].value == 14 && cards[1].value == 5) // Ace-low straight
                    continue;
                return false;
            }
        }
        return true;
    }

    public static HandResult FindBestHand(Player player, List<Card> tableCards)
    {
        List<Card> allCards = new List<Card>(player.playerCards);
        allCards.AddRange(tableCards);

        var combinations = CardCombinatorics.GetAllCombinations(allCards);


        HandResult bestHand = null;

        foreach (var combination in combinations)
        {
            var handResult = EvaluateHand(combination);
            if (bestHand == null || CompareHands(handResult, bestHand) > 0)
            {
                bestHand = handResult;
            }
        }

        return bestHand;
    }

    // Porównanie wyników dwóch graczy
    public static int CompareHands(HandResult hand1, HandResult hand2)
    {
        if (hand1.Rank > hand2.Rank)
            return 1;
        if (hand1.Rank < hand2.Rank)
            return -1;

        // W przypadku remisu porównaj wartości
        for (int i = 0; i < hand1.TiebreakerValues.Count; i++)
        {
            if (hand1.TiebreakerValues[i] > hand2.TiebreakerValues[i])
                return 1;
            if (hand1.TiebreakerValues[i] < hand2.TiebreakerValues[i])
                return -1;
        }
        return 0; // Remis
    }
}
