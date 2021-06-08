using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject playcardsObj;
    [SerializeField] GameObject deckObj;
    [SerializeField] GameObject groundObj;

    private List<Card> Deck = new List<Card>();
    private List<Card> UnshuffledDeck = new List<Card>();
    private List<Card> Ground = new List<Card>();
    private List<List<Card>> Playground = new List<List<Card>>();
    private const string BACK_OF_A_CARD_SPRITE_NAME = "Red Back of a card";
    private const string EMPTY_DECK_SPRTIE_NAME = "Blue Back of a card";
    private int remainingRefreshes;

    public static int moveCount;
    public static int score;

    private int initialMoveCount;
    private int initialScore;
    private void Start()
    {
        moveCount = 0;
        score = 0;
        initialScore = score;
        initialMoveCount = moveCount;
        remainingRefreshes = Settings.deckRefreshCount;
        UnshuffledDeck = CreateADeck();
        DealPlayCards();
        DealDeck();
    }
    private void Update()
    {
        if (moveCount != initialMoveCount)
        {
            initialMoveCount = moveCount;
            // show in the ui
        }

        if (score != initialScore)
        {
            score = initialScore;
            // show in the ui
        }
    }

    private List<Card> CreateADeck()
    {
        var deck = new List<Card>();
        for (int i = 0; i < 52; i++)
        {
            var card = new Card();
            card.ID = i;

            int cardValueIndex = card.ID % 13;
            card.Value = cardValueIndex switch
            {
                0 => 13,
                _ => cardValueIndex
            };


            int cardSuitIndex = (int)(card.ID / 13.1f);
            card.Suit = cardSuitIndex switch
            {
                0 => "H",
                1 => "C",
                2 => "S",
                3 => "D",
                _ => ""
            };

            card.ImageName = $"{card.Suit}{card.Value}";

            deck.Add(card);
        }
        return deck;
    }
    private Card Draw()
    {
        int cardID = Random.Range(0, UnshuffledDeck.Count);
        var card = UnshuffledDeck[cardID];
        UnshuffledDeck.Remove(card);
        return card;
    }
    private void DealPlayCards()
    {
        for (int column = 1; column < 8; column++)
        {
            Playground.Add(new List<Card>());
            for (int piece = 0; piece < column; piece++)
            {
                var columnIndex = column - 1;
                var card = Draw();

                Playground[columnIndex].Add(card);
                var cardObj = Instantiate(cardPrefab) as GameObject;
                cardObj.transform.SetParent(playcardsObj.transform.GetChild(columnIndex).transform);
                cardObj.name = card.ImageName;
                if (column == piece + 1)
                {
                    cardObj.GetComponent<CardMoveControl>().enabled = true;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.ImageName);
                }
                else
                {
                    cardObj.GetComponent<CardMoveControl>().enabled = false;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                }
            }
        }
    }
    private void DealDeck()
    {
        int count = UnshuffledDeck.Count;
        for (int i = 0; i < count; i++)
        {
            var card = Draw();
            Deck.Add(card);
            var cardObj = Instantiate(cardPrefab) as GameObject;
            cardObj.transform.SetParent(deckObj.transform);
            cardObj.transform.position = deckObj.transform.position;
            cardObj.name = card.ImageName;
            cardObj.GetComponent<CardMoveControl>().enabled = false;
            cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
            cardObj.AddComponent<Button>();
            cardObj.GetComponent<Button>().onClick.AddListener(delegate { DealFromDeck(); });
        }
    }
    private void DealFromDeck()
    {
        int cardsToDeal = Settings.drawingCardCount;
        for (int i = 0; i < cardsToDeal; i++)
        {
            int lastCardInDeckIndex = deckObj.transform.childCount - 1;
            var topCard = deckObj.transform.GetChild(lastCardInDeckIndex);

            var cardInList = Deck.Find(x => x.ImageName == topCard.name);
            Deck.Remove(cardInList);
            Ground.Add(cardInList);

            topCard.transform.SetParent(groundObj.transform);
            topCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(topCard.name);
            topCard.GetComponent<CardMoveControl>().enabled = true;
            topCard.GetComponent<Button>().enabled = false;

            if (groundObj.transform.childCount > 3)
            {
                for (int k = 0; k < groundObj.transform.childCount - 3; k++)
                {
                    groundObj.transform.GetChild(k).gameObject.SetActive(false);
                }
            }
        }
    }

    public void RefreshDeck()
    {
        if (remainingRefreshes > 0)
        {
            remainingRefreshes--;
            int k = groundObj.transform.childCount;
            for (int i = 0; i < k; i++)
            {
                var bottomCard = groundObj.transform.GetChild(0);
                bottomCard.gameObject.SetActive(true);
                var cardInList = Ground.Find(x => x.ImageName == bottomCard.name);
                Ground.Remove(cardInList);
                Deck.Add(cardInList);

                bottomCard.transform.SetParent(deckObj.transform);
                bottomCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                bottomCard.GetComponent<CardMoveControl>().enabled = false;
                bottomCard.GetComponent<Button>().enabled = true;
            }
        }
        else if (remainingRefreshes == 0)
        {
            deckObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(EMPTY_DECK_SPRTIE_NAME);
        }
    }
}