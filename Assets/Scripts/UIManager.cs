using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

[System.Serializable]
public class CardArea
{
    public List<PlayerImages> players;
}

[System.Serializable]
public class PlayerImages
{
    public List<UnityEngine.UI.Image> cards;
}

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public static TextMeshProUGUI gameText;
    public static Button playButton;
    public static Button flipButton;
    public static Button endTurnButton;
    public static Button targetSelectButton;
    public static GameObject selectHand;
    public static GameObject selectField;
    public static GameObject handArea;
    public static GameObject turnStart;
    public static AudioSource soundClick1;
    public static AudioSource soundClick2;
    public static AudioSource soundUI;
    public static AudioSource soundError;
    public static AudioSource soundTerrain;
    public static AudioSource soundFlip;
    public static Text targetDescription;
    public static Text[,] scoresUI = new Text[3, 2];


    public GameEngine gameEngine;

    public GameObject[] uIElements;
    public Button[] areaButtons;
    public Image[] areaImages;
    public Image cardPreview;
    public Sprite[] cardImages;

    public CardArea[] areas;
    public GameObject[] cardsInHand;
    public Sprite[] backgroundImages;


    public static int selectedCardHandId = -1;
    public static Card selectedCardHand;
    public static Card selectedCardField;
    public static int selectedArea = -1;

    public static int currentPlayer;
    private Card playedCard;
    private bool animationInProgress;
    private bool cardPlayed;
    private Image cardPreviewPlayed;
    private List<Card>[,] factionAreas;
    private int[,] scores = new int[3, 2];

    void GetComponents()
    {
        gameText = GameObject.Find("GameText").GetComponent<TextMeshProUGUI>();

        playButton = GameObject.Find("ButtonPlay").GetComponent<UnityEngine.UI.Button>();
        flipButton = GameObject.Find("ButtonFlip").GetComponent<UnityEngine.UI.Button>();
        endTurnButton = GameObject.Find("ButtonEnd").GetComponent<UnityEngine.UI.Button>();
        targetSelectButton = GameObject.Find("ButtonSelect").GetComponent<UnityEngine.UI.Button>();

        selectHand = GameObject.Find("SelectHand");
        handArea = GameObject.Find("HandArea");
        selectField = GameObject.Find("SelectField");
        turnStart = GameObject.Find("TurnStart");

        targetDescription = GameObject.Find("TargetDescription").GetComponent<Text>();
        cardPreviewPlayed = GameObject.Find("CardPreviewPlayed").GetComponent<Image>();

        soundClick1 = GameObject.Find("ClickSound1").GetComponent<AudioSource>();
        soundClick2 = GameObject.Find("ClickSound2").GetComponent<AudioSource>();
        soundUI = GameObject.Find("UISound").GetComponent<AudioSource>();
        soundError = GameObject.Find("ErrorSound").GetComponent<AudioSource>();
        soundTerrain = GameObject.Find("TerrainSound").GetComponent<AudioSource>();
        soundFlip = GameObject.Find("FlipSound").GetComponent<AudioSource>();

        selectField.SetActive(false);
        playButton.gameObject.SetActive(false);
        flipButton.gameObject.SetActive(false);
        targetDescription.gameObject.SetActive(false);
        turnStart.SetActive(false);

        playButton.onClick.AddListener(OnPlay);
        flipButton.onClick.AddListener(OnFlip);

        factionAreas = GameEngine.field.factionAreas;

        for(int a = 0;a<3;a++)
            for(int p = 0; p < 2; p++)
            {
                scoresUI[a, p] = GameObject.Find($"P{p}A{a}Score").GetComponent<Text>();
                scoresUI[a, p].text = $"{scores[a,p]}";
            }
                
    }

    void Start()
    {
        GetComponents();

        SwitchPlayerUI();
        currentPlayer = 0;
        Image backGroundImage = GameObject.Find("OffScreen").GetComponent<Image>();
        backGroundImage.sprite = backgroundImages[currentPlayer];

        cardPlayed = false;
        ShowHand();
        
        UpdatePlayerInfo();
    }

    private void CalculateScore()
    {
        List<Card>[,] factionAreas = GameEngine.field.factionAreas;
        scores = new int[3, 2];

        for(int a=0;a<3;a++)
            for(int p = 0; p < 2; p++)
            {
                if (GameEngine.field.CardExists(13, a, GameEngine.players[p]))
                {
                    if (a == 1)
                    {
                        scores[0, p] += 3;
                        scores[2, p] += 3;
                    }
                    else
                        scores[1, p] += 3;
                        
                }
                for (int c = 0; c < factionAreas[a, p].Count; c++)
                {
                    int power = factionAreas[a, p][c].Power;
                    if (factionAreas[a, p][c].IsFlipped)
                        power = 2;

                    if (GameEngine.field.CardExists(4, a, GameEngine.players[p]) && c < factionAreas[a, p].Count - 1)
                    {
                        for (int i = c + 1; i < factionAreas[a, p].Count; i++)
                        {
                            if (factionAreas[a, p][i].Id == 4)
                            {
                                power = 4;
                                break;
                            }
                        }
                    }
                    else if (GameEngine.field.CardExists(8, GameEngine.players[p]))
                    {
                        if (factionAreas[a, p][c].IsFlipped)
                            power = 4;
                    }
                    scores[a, p] += power;
                }
            }
                

        for (int a = 0; a < 3; a++)
            for (int p = 0; p < 2; p++)
            {
                scoresUI[a, p].text = scores[a, p].ToString();
                if (scores[a, p] > scores[a, (p + 1) % 2])
                    scoresUI[a, p].color = Color.green;
                else if (scores[a, p] < scores[a, (p + 1) % 2])
                    scoresUI[a, p].color = Color.red;
                else
                    scoresUI[a, p].color = Color.white;
            }
    }

    private void LateUpdate()
    {
        CalculateScore();
        gameText.text = "Teren:\n";
        for (int p = 0; p < 2; p++)
        {
            gameText.text += $"\nPlayer{p}\n";
            for (int a = 0; a < 3; a++)
            {
                gameText.text += $"=-=- Area{a} -=-=\n";
                if (factionAreas[a, p].Count == 0)
                    gameText.text += "Prazan Teren\n";
                for (int c = 0; c < factionAreas[a, p].Count; c++)
                {
                    gameText.text += " " + factionAreas[a, p][c].Name + "\n";
                }
            }
        }

        gameText.text += $"\n\n P1 Sasuke:{GameEngine.players[0].effects[0]}\nP2 Sasuke:{GameEngine.players[1].effects[0]}\n";
        if(playedCard != null)
            gameText.text += $"playedCard: {playedCard.Name}\n";
        else
            gameText.text += $"playedCard: null\n";
    }

    public void OnCardHandSelect(int index)
    {
        selectedCardHandId = index;

        if (index == -1)
        {
            playButton.gameObject.SetActive(false);
            flipButton.gameObject.SetActive(false);
            return;
        }
        
        selectedCardHand = GameEngine.players[currentPlayer].Hand[selectedCardHandId];

        if (selectedCardHand.Playable(GameEngine.players[currentPlayer],selectedArea) && !cardPlayed)
            playButton.gameObject.SetActive(true);
        else
            playButton.gameObject.SetActive(false);
        flipButton.gameObject.SetActive(true);

        ShowCard(selectedCardHand);
    }

    void OnPlay()
    {
        if (selectedCardHandId == -1)
        {
            Debug.Log("Moras izabrati kartu iz ruke.");
            return;
        }

        cardPlayed = true;
        soundClick2.Play();

        Player player = GameEngine.players[currentPlayer];
        Card card = player.Hand[selectedCardHandId];

        if (card.Playable(player, selectedArea))
        {
            player.Hand.Remove(card);
            factionAreas[selectedArea, player.PlayerID].Add(card);
            ShowField();
            PlayCard(player, card, selectedArea);
        }

        SelectArea(-1);
        selectedCardHandId = -1;
        ShowHand();
        OnCardHandSelect(selectedCardHandId);

        UpdatePlayerInfo();
    }

    public void PlayCard(Player player, Card card, int area)    //efekti nakon sto karta dodje na teren
    {
        card.Player = player;
        card.Area = area;
        card.Area = area;
        playedCard = card;

        if (player != null && card != null)
        {
            if (player.PlayerID != currentPlayer && card.ActionCard && !card.IsFlipped)
            {
                SwitchPlayerUI();
                selectedCardField = card;
            }
        }
        else if(player == null)
        {
            Debug.Log("Player == null");
            return;
        }
        else
        {
            Debug.Log("Card == null");
            return;
        }


        GameObject image = areas[area].players[player.PlayerID].cards[GameEngine.field.totalCards(area,player)-1].gameObject;
        
        

        LeanTween.moveLocalX(image, 30f, 0.05f)
            .setLoopPingPong(3)
            .setEase(LeanTweenType.easeInBounce);


        if (card.IsFlipped)
            return;

        //efekti karata ispod

        if (card.Id == 3 || card.Id == 9 || card.Id == 15)
        {
            Debug.Log("odigrana flip karta");
            if ((AdjecentAreas(area).Length == 1 && GameEngine.field.totalCards(1) > 0) || (AdjecentAreas(area).Length == 2 && (GameEngine.field.totalCards(0) + GameEngine.field.totalCards(2)) > 0))
            {
                SwapUiElements();
            }
            else
            {
                Debug.Log("Na susednom terenu nema nijedna karta.");
            }
        }
        else if(card.Id == 7)
        {
            Debug.Log("Odigrana Historia");
            SwapUiElements();
        }
        else if(card.Id == 2)
        {
            SwapUiElements();
        }
        else if (card.Id == 5)
        {
            SwapUiElements();
        }
        else if (card.Id == 1)
        {
            if ( (area==1 && (GameEngine.field.totalCards(0,player)+GameEngine.field.totalCards(2, player) == 8)) || ((area == 0 || area == 2) && (GameEngine.field.totalCards(1, player) == 4)))
            {
                Debug.Log("Nema dovoljno mesta na terenu.");
                return;
            }
            else if(GameEngine.deck.cards.Count == 0)
            {
                Debug.Log("Nema vise karata u spilu.");
                return;
            }
                
            SwapUiElements();
            cardPreviewPlayed.sprite = cardImages[GameEngine.deck.cards[0].Id];
            cardPreview.sprite = cardImages[GameEngine.deck.cards[0].Id];
        }

    }

    public void OnFlip()
    {
        if (selectedCardHandId == -1)
        {
            Debug.Log("Moras izabrati kartu iz ruke.");
            return;
        }
        
        soundFlip.Play();

        GameObject imageOfCard = GameObject.Find($"Card{selectedCardHandId}");

        if (!animationInProgress)
        {
            animationInProgress = true;
            LeanTween.rotateAroundLocal(imageOfCard, Vector3.forward, 360f, 0.2f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                animationInProgress = false;
            });
        }
        

        GameEngine.players[currentPlayer].Hand[selectedCardHandId].Flip();
        OnCardHandSelect(selectedCardHandId);
        ShowHand();
    }

    public Image GetFieldImage(Card card)
    {
        for(int a = 0; a < 3; a++)
        {
            for(int p = 0; p < 2; p++)
            {
                for(int c = 0; c < factionAreas[a,p].Count; c++)
                {
                    if(factionAreas[a, p][c].Id == card.Id)    //error
                        return areas[a].players[p].cards[c];
                }
            }
        }
        Debug.Log("Ne postoji ta karta flag4");
        return null;
    }

    public void TargetSelect()
    {
        Debug.Log("Kliknuto Izaberi metu");
        if (selectedCardField != null && GameEngine.field.isOnTopOfArea(selectedCardField) && playedCard.Id != 1)   // GameEngine.field.isOnTopOfArea(selectedCardField)
        {
            if (playedCard.Id == 3 || playedCard.Id == 9 || playedCard.Id == 15)    //  Rock Lee, Armin, Scar
                if (AdjecentAreas(playedCard.Area).Contains(selectedCardField.Area))
                {
                    selectedCardField.Flip();
                    soundFlip.Play();
                    
                    Image imageOfCard = GetFieldImage(selectedCardField);

                    if (!animationInProgress)
                    {
                        animationInProgress = true;
                        LeanTween.rotateAroundLocal(imageOfCard.gameObject, Vector3.forward, 360f, 0.2f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            animationInProgress = false;
                            PlayCard(selectedCardField.Player, selectedCardField, selectedCardField.Area);
                        });
                    }

                    ShowField();
                    SwapUiElements();
                }
                else
                {
                    soundError.Play();
                    targetDescription.text = "Mozes okrenuti kartu samo sa susednog terena";
                }
            else if (playedCard.Id == 7) //  Historia
            {
                if (selectedArea == -1)
                {
                    soundError.Play();
                    targetDescription.text = "Moras izabrati teren na koji ce preci karta.";
                }
                else if (GameEngine.field.totalCards(selectedArea, GameEngine.players[currentPlayer]) == 3)
                {
                    soundError.Play();
                    targetDescription.text = "Na ovom terenu ima previse karata.";
                }
                else if (selectedCardField.Player.PlayerID != currentPlayer)
                {
                    soundError.Play();
                    targetDescription.text = "Moras izabrati jednu od svojih karata.";
                }
                else
                {
                    //areas[selectedCardField.Area].players[currentPlayer].cards[GameEngine.field.totalCards(selectedCardField.Area, GameEngine.players[currentPlayer]) - 1].gameObject.SetActive(false);
                    GetFieldImage(selectedCardField).gameObject.SetActive(false);
                    factionAreas[selectedCardField.Area, currentPlayer].RemoveAt(factionAreas[selectedCardField.Area, currentPlayer].Count - 1);
                    //TODO dodavanje karte na drugi teren
                    selectedCardField.Area = selectedArea;
                    factionAreas[selectedCardField.Area, currentPlayer].Add(selectedCardField);
                    GetFieldImage(selectedCardField).gameObject.SetActive(true);

                    selectedCardField.Area = selectedArea;
                    ShowField();
                    SwapUiElements();
                }

            }   //  Historia
            else if (playedCard.Id == 2)
            {
                selectedCardField.Flip();
                soundFlip.Play();
                
                Image imageOfCard = GetFieldImage(selectedCardField);

                if (!animationInProgress)
                {
                    animationInProgress = true;
                    LeanTween.rotateAroundLocal(imageOfCard.gameObject, Vector3.forward, 360f, 0.2f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() =>
                    {
                        animationInProgress = false;
                        PlayCard(selectedCardField.Player, selectedCardField, selectedCardField.Area);
                    });
                }

                ShowField();
                SwapUiElements();
            }   //  Sakura
            else if (playedCard.Id == 5)
            {
                if (selectedCardField.Player == GameEngine.players[currentPlayer])
                {
                    if (cardPlayed)
                        GameEngine.players[(currentPlayer + 1) % 2].setEffect(0, true);
                    selectedCardField.Flip();
                    soundFlip.Play();
                    PlayCard(selectedCardField.Player, selectedCardField, selectedCardField.Area);
                    Image imageOfCard = GetFieldImage(selectedCardField);

                    if (!animationInProgress)
                    {
                        animationInProgress = true;
                        LeanTween.rotateAroundLocal(imageOfCard.gameObject, Vector3.forward, 360f, 0.2f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            animationInProgress = false;
                        });
                    }
                    ShowField();
                    SwapUiElements();
                }
                else
                {
                    targetDescription.text = "Moras izabrati neku od svojih karata";
                }
            }   //  Sasuke


        }
        else if (playedCard.Id == 1)     //  Hinata
        {
            if (selectedArea == -1)
            {
                soundError.Play();
                targetDescription.text = "Moras izabrati teren.";
            }
            else if (GameEngine.field.totalCards(selectedArea, GameEngine.players[currentPlayer]) == 4)
            {
                soundError.Play();
                targetDescription.text = "Na ovom terenu ima previse karata.";
            }
            else if (!AdjecentAreas(playedCard.Area).Contains(selectedArea))
            {
                soundError.Play();
                targetDescription.text = "Moras odabrati susedan teren.";
            }
            else if(GameEngine.deck.cards.Count == 0)
            {
                soundError.Play();
                Debug.Log("Spil je prazan");
                ShowField();
                SwapUiElements();
            }
            else
            {
                Card hinataCard = GameEngine.deck.cards[0];
                GameEngine.deck.cards.Remove(hinataCard);
                hinataCard.Area = selectedArea;
                hinataCard.Player = GameEngine.players[currentPlayer];
                hinataCard.Flip();
                factionAreas[selectedArea, currentPlayer].Add(hinataCard);
                GetFieldImage(hinataCard).gameObject.SetActive(true);

                PlayCard(hinataCard.Player, hinataCard, hinataCard.Area);

                ShowField();
                SwapUiElements();
            }

        }
        else
        {
            soundError.Play();
            targetDescription.text = "Moras odabrati metu";
        }

    }

    public void UpdatePlayerInfo()
    {
        debugText.text = "Teren:\n";
        for (int a = 0; a < 3; a++)
        {
            for (int p = 0; p < 2; p++)
            {
                for (int c = 0; c < factionAreas[a, p].Count; c++)
                {
                    debugText.text += "a=" + a + " p=" + p + " " + factionAreas[a, p][c] + "\n";
                }
            }
        }
    }

    public void ShowCard(Card card)
    {
        cardPreview.sprite = CardSprite(card);
        PreviewCardAnimation();

        ShowHand();
    }

    public void PreviewCardAnimation()
    {
        if (animationInProgress)
            return;

        animationInProgress = true;

        Vector3 startingPosition = cardPreview.transform.position;

        LeanTween.move(cardPreview.gameObject, cardPreview.transform.position + new Vector3(0f, 290f, 0f), 0.4f)
            .setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(() =>
            {
                LeanTween.move(cardPreview.gameObject, cardPreview.transform.position - new Vector3(0f, 290f, 0f), 0.4f)
                    .setEase(LeanTweenType.easeOutExpo)
                    .setOnComplete(() =>
                    {
                        animationInProgress = false;
                    });
                
            });
    }

    public Sprite CardSprite(Card card)
    {
        if (card.IsFlipped)
            return cardImages[0];
        else
            return cardImages[card.Id];
    }

    public void ShowField()
    {
        for(int a = 0;a < 3;a++)
            for(int p = 0;p < 2;p++)
                for (int c = 0; c < factionAreas[a, p].Count; c++)
                {
                    areas[a].players[p].cards[c].gameObject.SetActive(true);
                    areas[a].players[p].cards[c].sprite = CardSprite(factionAreas[a, p][c]);
                }
    }

    public void SwitchPlayerUI()
    {
        currentPlayer = (currentPlayer + 1) % GameEngine.players.Length;
        soundUI.Play();
        ResetAllSelections();

        Image backGroundImage = GameObject.Find("OffScreen").GetComponent<Image>();
        backGroundImage.sprite = backgroundImages[currentPlayer];

        GameObject[] cardPlayAreas = GameObject.FindGameObjectsWithTag("CardPlayArea");

        foreach (GameObject area in cardPlayAreas)
        {
            RectTransform rectTransform = area.GetComponent<RectTransform>();
            VerticalLayoutGroup layout = area.GetComponent<VerticalLayoutGroup>();
            if (rectTransform != null)
            {
                Vector2 currentAnchoredPosition = rectTransform.anchoredPosition;
                currentAnchoredPosition.y = -currentAnchoredPosition.y;
                rectTransform.anchoredPosition = currentAnchoredPosition;
                if(layout != null)
                {
                    if (layout.childAlignment == TextAnchor.UpperCenter)
                        layout.childAlignment = TextAnchor.LowerCenter;
                    else
                        layout.childAlignment = TextAnchor.UpperCenter;
                }
                
            }
        }

        ShowHand();
    }

    public void SelectArea(int area)
    {
        selectedArea = area;
        for (int i = 0; i < areaImages.Length; i++)
        {
            areaImages[i].transform.localScale = Vector3.one;
            areaImages[i].color = Color.gray;
        }
        if (area != -1)
        {
            LeanTween.scale(areaImages[area].GetComponent<RectTransform>(), Vector3.one * 1.1f, 0.1f);
            //areaImages[area].transform.localScale = Vector3.one * 1.1f;
            areaImages[area].color = Color.white;
        }

        if (selectedCardHand!=null && selectedCardHand.Playable(GameEngine.players[currentPlayer],selectedArea) && cardPlayed==false)
        {
            playButton.gameObject.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(false);
        }

    }

    public void ShowHand()
    {
        for (int i = 0; i < 6; i++)
        {
            cardsInHand[i].SetActive(false);
        }
        for (int i = 0; i < GameEngine.players[currentPlayer].Hand.Count; i++)
        {
            cardsInHand[i].SetActive(true);
            Image imageOfCardHand = cardsInHand[i].GetComponent<Image>();
            imageOfCardHand.sprite = CardSprite(GameEngine.players[currentPlayer].Hand[i]);
            if (selectedCardHandId == i)
            {
                LeanTween.scale(imageOfCardHand.gameObject, new Vector3(1.35f, 1.35f, 1.35f), 0.2f);
                imageOfCardHand.color = Color.white;
                imageOfCardHand.transform.rotation = Quaternion.Euler(0, 0, 5);
            }
            else
            {
                imageOfCardHand.transform.localScale = Vector3.one;
                imageOfCardHand.color = Color.grey;
                imageOfCardHand.transform.rotation = Quaternion.Euler(Vector3.zero);
            }

        }
    }

    public void SwapUiElements()
    {
        selectHand.SetActive(!selectHand.activeInHierarchy);
        selectField.SetActive(!selectField.activeInHierarchy);
        handArea.SetActive(!handArea.activeInHierarchy);
        targetDescription.gameObject.SetActive(!targetDescription.gameObject.activeInHierarchy);
        if(targetDescription.gameObject.activeInHierarchy)
        {
            cardPreviewPlayed.sprite = cardImages[playedCard.Id];
            cardPreview.sprite = cardImages[playedCard.Id];
            targetDescription.text = "";
        }
        
    }

    public int[] AdjecentAreas(int area)
    {
        int[] adjecentAreas;
        if (area == 1)
        {
            adjecentAreas = new int[2];
            adjecentAreas[0] = 0;
            adjecentAreas[1] = 2;
        }
        else
        {
            adjecentAreas = new int[1];
            adjecentAreas[0] = 1;
        }
        return adjecentAreas;
    }

    public void ResetAllSelections()
    {
        SelectArea(-1);
        selectedCardField = null;
        selectedCardHand = null;
        selectedCardHandId = -1;
        cardPreview.sprite = cardImages[19];
    }

    public void EndTurn()
    {
        
        if(GameEngine.turn % 2 == currentPlayer)
        {
            SwitchPlayerUI();
        }

        soundUI.Play();
        ResetAllSelections();


        turnStart.SetActive(true);
        handArea.SetActive(false);
        selectHand.SetActive(false);

        GameEngine.turn++;

        cardPlayed = false;
        
    }

    public void StartTurn()
    {
        handArea.SetActive(true);
        selectHand.SetActive(true);
        turnStart.SetActive(false);

        if (GameEngine.players[currentPlayer].effects[0])
        {
            playedCard = new Card(5);
            selectedCardHand = playedCard;
            SwapUiElements();
            GameEngine.players[currentPlayer].setEffect(0, false);
        }
        
    }

}