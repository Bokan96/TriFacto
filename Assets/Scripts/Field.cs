using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Field
{
    public List<Card>[,] factionAreas;
    public int[,] scores;
    private Player[] winningPlayer;

    public List<Card>[,] FactionAreas { get => factionAreas; set => factionAreas = value; }

    public Field()
    {
        factionAreas = new List<Card>[3, 2];
        scores = new int[3, 2];
        winningPlayer = new Player[3];

        for (int a = 0; a < 3; a++)
        {
            for (int p = 0; p < 2; p++)
            {
                factionAreas[a, p] = new List<Card>();
            }
            winningPlayer[a] = null;
        }
    }

    public void DisplayFieldState()
    {
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Area {i + 1}:");

            if (winningPlayer[i] != null)
            {
                Debug.Log($"Winning Player: {winningPlayer[i].PlayerID}");
            }

            for (int j = 0; j < 2; j++)
            {
                Debug.Log($"Player {j + 1}:");

                foreach (Card card in factionAreas[i, j])
                {
                    card.DisplayCardInfo();
                }

                Debug.Log($"Total Power: {scores[i, j]}");
            }

            Debug.Log("--------------");
        }
    }

    public bool CardExists(int cardId)
    {
        for(int a = 0; a < 3; a++)
        {
            for(int p=0; p < 2; p++)
            {
                for(int c=0; c < factionAreas[a, p].Count; c++)
                {
                    if (factionAreas[a, p][c].Id == cardId && !factionAreas[a, p][c].IsFlipped)
                        return true;
                }
            }
        }
        return false;
    }

    public bool CardExists(int cardId, int area)
    {
        for (int p = 0; p < 2; p++)
        {
            for (int c = 0; c < factionAreas[area, p].Count;c++)
            {
                if (factionAreas[area, p][c].Id == cardId && !factionAreas[area, p][c].IsFlipped)
                    return true;
            }
        }
        return false;
    }

    public bool CardExists(int cardId, Player player)
    {
        for (int a = 0; a < 3; a++)
        {
            for (int c = 0; c < factionAreas[a, player.PlayerID].Count; c++)
            {
                if (factionAreas[a, player.PlayerID][c].Id == cardId && !factionAreas[a, player.PlayerID][c].IsFlipped)
                    return true;
            }
        }
        return false;
    }

    public bool CardExists(int cardId, int area, Player player)
    {
        for (int c = 0; c < factionAreas[area, player.PlayerID].Count; c++)
        {
            if (factionAreas[area, player.PlayerID][c].Id == cardId && !factionAreas[area, player.PlayerID][c].IsFlipped)
                return true;
        }
        return false;
    }

    public bool isOnTopOfArea(Card card)
    {
        for(int a=0;a < 3;a++)
        {
            for(int p=0; p<2; p++)
            {
                if (factionAreas[a, p].Count > 0 && factionAreas[a, p][factionAreas[a, p].Count - 1].Id == card.Id)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool isOnTopOfArea(Card card, int area)
    {
        for (int p = 0; p < 2; p++)
        {
            if (factionAreas[area, p].Count > 0 && factionAreas[area, p][factionAreas[area, p].Count - 1].Id == card.Id)
                return true;
        }
        return false;
    }

    public bool isOnTopOfArea(Card card, int area, Player player)
    {
        if (factionAreas[area, player.PlayerID].Count > 0 && factionAreas[area, player.PlayerID][factionAreas[area, player.PlayerID].Count - 1].Id == card.Id)
            return true;
        return false;
    }

    public int totalCards()
    {
        int total = 0;

        for (int a = 0; a < 3; a++)
            for (int p = 0; p < 2; p++)
                total += factionAreas[a, p].Count;

        return total;
    }

    public int totalCards(int area)
    {
        int total = 0;
        for (int p = 0; p < 2; p++)
            total += factionAreas[area, p].Count;
        return total;
    }

    public int totalCards(int area, Player player)
    {
            return factionAreas[area, player.PlayerID].Count;
    }

    public int totalCards(Player player)
    {
        int total = 0;
        for (int a = 0; a < 3; a++)
            total += factionAreas[a, player.PlayerID].Count;
        return total;
    }


}
