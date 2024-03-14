using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck
{
    public List<Card> cards;
    public Deck()
    {
        cards = new List<Card>();

        for (int i = 1; i <= 18; i++)
        {
            Card newCard = new Card(i);
            cards.Add(newCard);
        }
    }

    public Deck(int[] cardsIndluded)
    {
        cards = new List<Card>();
        foreach (int card in cardsIndluded)
        {
            cards.Add(new Card(card));
        }

        for (int i = 1; i <= 18; i++)
        {
            if (!cardsIndluded.Contains(i) && cards.Count<15)
                cards.Add(new Card(i));
        }
    }

    public void Shuffle()
    {
        System.Random rng = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DealCardToPlayer(Player player)
    {
        if (cards.Count == 0)
        {
            Debug.Log("Spil je prazan.");
            return null;
        }

        Card dealtCard = cards[0];
        dealtCard.Player = player;
        player.AddCardToHand(dealtCard);
        cards.RemoveAt(0);

        return dealtCard;
    }

}
