using System.Collections.Generic;
using UnityEngine;

public class PokerHandTest : MonoBehaviour
{
    void Start()
    {
        RunTests();
    }

    void RunTests()
    {
        // Scenariusz 1: Royal Flush
        TestPlayerHand(
            new List<Card>
            {
                new Card(1, 14, Card.CardType.Ace, Card.CardColor.Spade), // As Pik
                new Card(2, 13, Card.CardType.King, Card.CardColor.Spade) // Król Pik
            },
            new List<Card>
            {
                new Card(3, 12, Card.CardType.Queen, Card.CardColor.Spade), // Dama Pik
                new Card(4, 11, Card.CardType.Jack, Card.CardColor.Spade),  // Walet Pik
                new Card(5, 10, Card.CardType.Numerical, Card.CardColor.Spade), // 10 Pik
                new Card(6, 3, Card.CardType.Numerical, Card.CardColor.Diamond), // 3 Karo
                new Card(7, 5, Card.CardType.Numerical, Card.CardColor.Club) // 5 Trefl
            },
            "Royal Flush"
        );

        // Scenariusz 2: Full House
        TestPlayerHand(
            new List<Card>
            {
                new Card(8, 3, Card.CardType.Numerical, Card.CardColor.Heart), // 3 Kier
                new Card(9, 3, Card.CardType.Numerical, Card.CardColor.Diamond) // 3 Karo
            },
            new List<Card>
            {
                new Card(10, 3, Card.CardType.Numerical, Card.CardColor.Club), // 3 Trefl
                new Card(11, 5, Card.CardType.Numerical, Card.CardColor.Spade), // 5 Pik
                new Card(12, 5, Card.CardType.Numerical, Card.CardColor.Diamond), // 5 Karo
                new Card(13, 7, Card.CardType.Numerical, Card.CardColor.Club), // 7 Trefl
                new Card(14, 9, Card.CardType.Numerical, Card.CardColor.Heart) // 9 Kier
            },
            "Full House"
        );

        // Scenariusz 3: Straight
        TestPlayerHand(
            new List<Card>
            {
                new Card(15, 6, Card.CardType.Numerical, Card.CardColor.Club), // 6 Trefl
                new Card(16, 7, Card.CardType.Numerical, Card.CardColor.Heart) // 7 Kier
            },
            new List<Card>
            {
                new Card(17, 8, Card.CardType.Numerical, Card.CardColor.Diamond), // 8 Karo
                new Card(18, 9, Card.CardType.Numerical, Card.CardColor.Spade), // 9 Pik
                new Card(19, 10, Card.CardType.Numerical, Card.CardColor.Club), // 10 Trefl
                new Card(20, 3, Card.CardType.Numerical, Card.CardColor.Diamond), // 3 Karo
                new Card(21, 2, Card.CardType.Numerical, Card.CardColor.Club) // 2 Trefl
            },
            "Straight"
        );

        // Scenariusz 4: Flush
        TestPlayerHand(
            new List<Card>
            {
                new Card(22, 2, Card.CardType.Numerical, Card.CardColor.Heart), // 2 Kier
                new Card(23, 4, Card.CardType.Numerical, Card.CardColor.Heart) // 4 Kier
            },
            new List<Card>
            {
                new Card(24, 6, Card.CardType.Numerical, Card.CardColor.Heart), // 6 Kier
                new Card(25, 8, Card.CardType.Numerical, Card.CardColor.Heart), // 8 Kier
                new Card(26, 10, Card.CardType.Numerical, Card.CardColor.Heart), // 10 Kier
                new Card(27, 12, Card.CardType.Queen, Card.CardColor.Diamond), // Dama Karo
                new Card(28, 9, Card.CardType.Numerical, Card.CardColor.Spade) // 9 Pik
            },
            "Flush"
        );

        // Scenariusz 5: High Card
        TestPlayerHand(
            new List<Card>
            {
                new Card(29, 3, Card.CardType.Numerical, Card.CardColor.Spade), // 3 Pik
                new Card(30, 5, Card.CardType.Numerical, Card.CardColor.Club) // 5 Trefl
            },
            new List<Card>
            {
                new Card(31, 7, Card.CardType.Numerical, Card.CardColor.Heart), // 7 Kier
                new Card(32, 9, Card.CardType.Numerical, Card.CardColor.Diamond), // 9 Karo
                new Card(33, 11, Card.CardType.Jack, Card.CardColor.Spade), // Walet Pik
                new Card(34, 4, Card.CardType.Numerical, Card.CardColor.Heart), // 4 Kier
                new Card(35, 6, Card.CardType.Numerical, Card.CardColor.Diamond) // 6 Karo
            },
            "High Card"
        );
    }

    void TestPlayerHand(List<Card> playerCards, List<Card> tableCards, string expectedHand)
    {
        Player player = new Player();
        player.playerCards = playerCards;

        var result = PokerHandEvaluator.FindBestHand(player, tableCards);

        Debug.Log($"Oczekiwane: {expectedHand}, Wynik: {result.Description}");
        Debug.Assert(result.Description == expectedHand, $"Test nie przeszedl! Oczekiwane: {expectedHand}, Wynik: {result.Description}");
    }
}
