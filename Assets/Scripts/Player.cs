using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player
{
    public int PlayerID { get; private set; }
    public int CurrentHP { get; private set; }
    public List<Card> Hand { get; private set; }
    public bool[] effects {  get; private set; }    // 0-Sasuke

    public Player(int playerID, int startingHP)
    {
        PlayerID = playerID;
        CurrentHP = startingHP;
        Hand = new List<Card>();
        effects = new bool[2];
        effects[0] = false;
        effects[1] = false;
    }

    public void AddCardToHand(Card card)
    {
        card.Player = this;
        Hand.Add(card);
    }
    public void SortCardsInHandById()
    {
        Hand = Hand.OrderBy(card => card.Id).ToList();
    }

    public void DisplayPlayerInfo()
    {
        Debug.Log($"Player ID: {PlayerID}, HP: {CurrentHP}");
        Debug.Log("Cards in hand:");
        if (Hand.Count > 0)
        {
            foreach (Card card in Hand)
            {
                card.DisplayCardInfo();
            }
        }
        else
            Debug.Log("No cards in hand");
    }

    public void setEffect(int effect, bool state)
    {
        effects[effect] = state;
    }
}
