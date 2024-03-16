using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Card
{
    public int Id { get; set; }
    public int Power { get; set; }
    public int Faction { get; set; }

    public int Area {  get; set; }
    public bool IsFlipped { get; set; }
    public Player Player { get; set; }
    public String Name { get; set; }

    public bool ActionCard {  get; set; }

    public Card(int id, int power, int faction, bool isFlipped, String name)
    {
        Id = id;
        Power = power;
        Faction = faction;
        IsFlipped = isFlipped;
        Name = name;
        Player = null;
        Area = -1;
    }
    
    public Card(int id)
    {
        Id = id;
        Power = (id - 1) % 6 + 1;
        Faction = (id - 1) / 6;
        IsFlipped |= false;
        Player = null;
        Area = -1;
        Name = id switch
        {
            1 => "Hinata",
            2 => "Sakura",
            3 => "Rock Lee",
            4 => "Kakashi",         //4 Kakashi ispod njega 4
            5 => "Sasuke",
            6 => "Naruto",
            7 => "Historia",
            8 => "Erwin Smith",     //8 Erwin neotkrivene == 4
            9 => "Armin",
            10 => "Eren Yeager",
            11 => "Levi",
            12 => "Mikasa",
            13 => "May Cheng",      //13 May Cheng susedni tereni +3
            14 => "Armstrong",
            15 => "Scar",
            16 => "Roy Mustang",
            17 => "Alphonse Elric",
            18 => "Edvard Elric",
            _ => "Bezimena",
        };

        ActionCard = id switch
        {
            1 => true,
            2 => true,
            3 => true,
            4 => false,
            5 => true,
            6 => false,
            7 => true,
            8 => false,
            9 => true,
            10 => true,
            11 => false,
            12 => false,
            13 => false,
            14 => true,
            15 => true,
            16 => false,
            17 => false,
            18 => false,
            _ => false,
        };
    }


    public void DisplayCardInfo()
    {
        Debug.Log($"{Name}:  POW {Power}| FAC {Faction}");
    }

    public void Flip()
    {
        IsFlipped = !IsFlipped;
    }

    public override string ToString()
    {
        char faction;
        if (Faction == 0)
            faction = 'C';
        else if (Faction == 1)
            faction = 'G';
        else if (Faction == 2)
            faction = 'B';
        else
            faction = 'X';
        return $"[{faction}-{Power}] {Name}";
    }

    public bool Playable(Player player, int area)
    {
        Field field = GameEngine.field;

        bool playable;

        if (area == -1 || player == null)
        {
            return false;
        }

        if (GameEngine.field.FactionAreas[area, player.PlayerID].Count >= 4)
        {
            UIManager.gameText.text = "Ne moze se odigrati vise od 4 karte na jedan teren.";
            return false;
        }


        //  Levi
        if (field.CardExists(11))
        {
            if ( field.CardExists(11, 1) && (area==0 || area==2) )
            {
                if (field.factionAreas[area,0].Count + field.factionAreas[area, 1].Count >= 3)
                {
                    UIManager.gameText.text = "Na ovo terenu ne moze biti vise od 3 karte";
                    return false;
                }
            }
            else if ( (field.CardExists(11, 0) || field.CardExists(11,2)) && area == 1)
            {
                if (field.factionAreas[area, 0].Count + field.factionAreas[area, 1].Count >= 3)
                {
                    UIManager.gameText.text = "Na ovo terenu ne moze biti vise od 3 karte";
                    return false;
                }
            }
        }


        if (IsFlipped)
        {
            if (field.CardExists(17))       // Alphonse Elric
            {
                playable = false;
                UIManager.gameText.text = "Karte se ne mogu odigrati neotkrivene ok je Alphonse na terenu.";
            }
            else
                playable = true;
        }
        else
        {
            if (Faction == area || player.effects[1] == true)   //  Armstrong
            {
                playable = true;
            }
            else if (field.CardExists(16,player) && Power <= 3)        //Roy mustang
            {
                UIManager.gameText.text = "Roy Mustang omogucava igranje karte na ovaj teren.";
                playable = true;
            }
            else
            {
                UIManager.gameText.text = "Karta mora biti iste boje kao teren.";
                playable = false;
            }
        }


        return playable;
    }

    public bool isOnTop()
    {
        for(int a=0;a<3;a++)
            for(int p=2;p<2;p++)
                if (GameEngine.field.factionAreas[a, p].Count > 0 && GameEngine.field.factionAreas[a, p][GameEngine.field.factionAreas[a, p].Count - 1].Id == Id)
                    return true;
        return false;
    }
}
