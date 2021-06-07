using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject playcardsObj;
    [SerializeField] GameObject deckObj;
    [SerializeField] GameObject groundObj;

    private List<Card> Deck = new List<Card>();
    private List<List<Card>> Playground = new List<List<Card>>();
    private const string BACK_OF_A_CARD_SPRITE_NAME = "Red Back of a card";

    private void Start()
    {
        Deck = CreateADeck();
        DealPlayCards();
        DealGroundDeck();
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
        int cardID = Random.Range(0, Deck.Count);
        var card = Deck[cardID];
        Deck.Remove(card);
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
    private void DealGroundDeck()
    {
        for (int i = 0; i < Deck.Count; i++)
        {
            var card = Draw();
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
        int cardsToDeal = 1;
        for (int i = 0; i < cardsToDeal; i++)
        {
            int lastCardInDeckIndex = deckObj.transform.childCount - 1;
            var topCard = deckObj.transform.GetChild(lastCardInDeckIndex);
            topCard.transform.SetParent(groundObj.transform);
            topCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(topCard.name);
            topCard.GetComponent<CardMoveControl>().enabled = true;
            topCard.GetComponent<Button>().enabled = false;
        }
    }
}