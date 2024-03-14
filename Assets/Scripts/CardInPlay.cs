using System;
using UnityEngine;
using UnityEngine.UI;

public class CardInPlay : MonoBehaviour
{
    public UIManager uIManager;
    public int player;
    public int area;
    public int card;

    public void OnClickCard2()
    {
        Card selectedCard;
        if (area == -1)
            selectedCard = UIManager.selectedCardHand;
        else
            selectedCard = GameEngine.field.factionAreas[area, player][card];

        UIManager.selectedCardField = selectedCard;

        if (UIManager.currentPlayer != player && selectedCard.IsFlipped==true)
            uIManager.cardPreview.sprite = uIManager.cardImages[0];
        else
            uIManager.cardPreview.sprite = uIManager.cardImages[selectedCard.Id];

        uIManager.ShowHand();
    }
}