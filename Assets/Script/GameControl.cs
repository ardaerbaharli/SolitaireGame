using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject playcardsObj;
    [SerializeField] GameObject deckObj;
    [SerializeField] GameObject acePanel;
    [SerializeField] GameObject groundObj;

    private List<Card> Deck = new List<Card>();
    private List<Card> UnshuffledDeck = new List<Card>();
    private List<Card> Ground = new List<Card>();
    private List<List<Card>> Playground = new List<List<Card>>();
    private static List<Move> Moves = new List<Move>();

    private const string BACK_OF_A_CARD_SPRITE_NAME = "Red Back of a card";
    private const string EMPTY_DECK_SPRTIE_NAME = "Blue Back of a card";

    private int remainingRefreshes;

    public static int moveCount;


    private void Start()
    {
        moveCount = 0;
        remainingRefreshes = Settings.deckRefreshCount;
        UnshuffledDeck = CreateADeck();
        DealPlayCards();
        DealDeck();
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
                    cardObj.GetComponent<CardMoveControl>().isFacingUp = true;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.ImageName);
                }
                else
                {
                    cardObj.GetComponent<CardMoveControl>().enabled = false;
                    cardObj.GetComponent<CardMoveControl>().isFacingUp = false;
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
            cardObj.GetComponent<CardMoveControl>().isFacingUp = false;
            cardObj.GetComponent<CardMoveControl>().enabled = false;
            cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
            cardObj.AddComponent<Button>();
            cardObj.GetComponent<Button>().onClick.AddListener(delegate { DealFromDeck(); });
        }
    }
    private void DealFromDeck()
    {
        moveCount++;
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
            topCard.GetComponent<CardMoveControl>().isFacingUp = true;
            topCard.GetComponent<Button>().enabled = false;

            Move move = new Move();
            move.Origin = deckObj.transform;
            move.Card = topCard.gameObject;
            move.Target = groundObj.transform;
            AddMove(move);

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
        if (deckObj.transform.childCount == 0)
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
                    bottomCard.GetComponent<CardMoveControl>().isFacingUp = false;
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
    public static void AddMove(Move move)
    {
        Moves.Add(move);
    }
    public void Undo()
    {
        var move = Moves.Last();
        if (move != null)
        {
            var card = move.Card;
            var origin = move.Origin;
            var target = move.Target;

            var newTarget = origin;

            card.transform.SetParent(newTarget);

            origin.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(origin); // set the spacing for the panel layout
            target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(target); // set the spacing for the panel layout

            LayoutRebuilder.ForceRebuildLayoutImmediate(card.transform.parent.GetComponent<RectTransform>()); // refresh layout

            Moves.Remove(move);
        }
    }
    public List<Step> ShowNextStep()
    {
        int firstPanelCount = playcardsObj.transform.childCount;

        // From playground cards => origin
        for (int i = 0; i < firstPanelCount; i++)
        {
            var firstPanel = playcardsObj.transform.GetChild(i);
            int firstPanelCardCount = firstPanel.transform.childCount;
            for (int k = 0; k < firstPanelCardCount; k++)
            {
                var firstCard = firstPanel.GetChild(k);
                if (firstCard.GetComponent<CardMoveControl>().isFacingUp)
                {
                    char firstCardSuit = firstCard.name[0];
                    int firstCardValue = int.Parse(firstCard.name.Substring(1));

                    if (firstPanel.childCount - 1 == k) // the card have to be on top in order to go to ace panel
                    {
                        int acesPanelCount = acePanel.transform.childCount;
                        for (int m = 0; m < acesPanelCount; m++)
                        {
                            var acesPanel = acePanel.transform.GetChild(m);
                            if (acesPanel.childCount > 0)
                            {
                                int lastAcesCardIndex = acesPanel.childCount - 1;

                                var lastCard = acesPanel.GetChild(lastAcesCardIndex);

                                char acesCardSuit = lastCard.name[0];
                                int acesCardValue = int.Parse(lastCard.name.Substring(1));
                                if (acesCardSuit == firstCardSuit && firstCardValue == acesCardValue + 1)
                                {
                                    Step step = new Step();
                                    step.Card = firstCard.gameObject;
                                    step.Target = acesPanel;
                                    return step.ToList();
                                }
                            }
                            else if (firstCardValue == 1)
                            {
                                Step step = new Step();
                                step.Card = firstCard.gameObject;
                                step.Target = acesPanel;
                                return step.ToList();
                            }
                        }
                    }


                    for (int j = 0; j < firstPanelCount; j++)
                    {
                        var secondPanel = playcardsObj.transform.GetChild(j);
                        if (firstPanel != secondPanel)
                        {
                            if (secondPanel.childCount > 0)
                            {
                                int secondPanelCardCount = secondPanel.childCount;
                                for (int n = 0; n < secondPanelCardCount; n++)
                                {
                                    var secondCard = secondPanel.GetChild(n);
                                    if (secondCard.GetComponent<CardMoveControl>().isFacingUp)
                                    {
                                        char secondCardSuit = secondCard.name[0];
                                        int secondCardValue = int.Parse(secondCard.name.Substring(1));
                                        if (firstCardValue == secondCardValue - 1 && IsPlaygroudCardSuitTrue(secondCardSuit, firstCardSuit))
                                        {
                                            List<Step> steps = new List<Step>();
                                            for (int z = firstPanelCardCount - 1; z >= firstCard.GetSiblingIndex(); z--)
                                            {
                                                Step step = new Step();
                                                step.Card = firstCard.gameObject;
                                                step.Target = secondPanel;
                                                steps.Add(step);
                                            }
                                            return steps;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // From ground card => origin
        if (groundObj.transform.childCount > 0)
        {
            int topGroundCardIndex = groundObj.transform.childCount - 1;
            var groundCard = groundObj.transform.GetChild(topGroundCardIndex);
            char groundCardSuit = groundCard.name[0];
            int groundCardValue = int.Parse(groundCard.name.Substring(1));

            for (int j = 0; j < firstPanelCount; j++)
            {
                int acesPanelCount = acePanel.transform.childCount;
                for (int m = 0; m < acesPanelCount; m++)
                {
                    var acesPanel = acePanel.transform.GetChild(m);
                    if (acesPanel.childCount > 0)
                    {
                        int lastAcesCardIndex = acesPanel.childCount - 1;

                        var lastCard = acesPanel.GetChild(lastAcesCardIndex);

                        char acesCardSuit = lastCard.name[0];
                        int acesCardValue = int.Parse(lastCard.name.Substring(1));
                        if (acesCardSuit == groundCardSuit && groundCardValue == acesCardValue + 1)
                        {
                            Step step = new Step();
                            step.Card = groundCard.gameObject;
                            step.Target = acesPanel;
                            return step.ToList();
                        }
                    }
                    else if (groundCardValue == 1)
                    {
                        Step step = new Step();
                        step.Card = groundCard.gameObject;
                        step.Target = acesPanel;
                        return step.ToList();
                    }
                }

                var firstPanel = playcardsObj.transform.GetChild(j);
                int firstPanelCardCount = firstPanel.transform.childCount;
                for (int k = 0; k < firstPanelCardCount; k++)
                {
                    var playingCard = firstPanel.GetChild(k);
                    if (playingCard.GetComponent<CardMoveControl>().isFacingUp)
                    {
                        char playingCardSuit = playingCard.name[0];
                        int playingCardValue = int.Parse(playingCard.name.Substring(1));
                        if (IsPlaygroudCardSuitTrue(playingCardSuit, groundCardSuit) && groundCardValue + 1 == playingCardValue)
                        {
                            Step step = new Step();
                            step.Card = groundCard.gameObject;
                            step.Target = firstPanel;
                            return step.ToList();
                        }
                    }
                }
            }
        }
        Step deck = new Step();
        deck.Card = null;
        deck.Target = deckObj.transform;
        return new List<Step>();
    }

    public void Help()
    {
        var steps = ShowNextStep();
        foreach (var item in steps)
        {
            item.Card.transform.SetParent(item.Target);
        }
    }

    private bool IsPlaygroudCardSuitTrue(char targetCardSuit, char cardSuit)
    {
        if (targetCardSuit == 'H' && (cardSuit == 'S' || cardSuit == 'C'))
            return true;
        else if (targetCardSuit == 'D' && (cardSuit == 'S' || cardSuit == 'C'))
            return true;
        else if (targetCardSuit == 'S' && (cardSuit == 'H' || cardSuit == 'D'))
            return true;
        else if (targetCardSuit == 'C' && (cardSuit == 'H' || cardSuit == 'D'))
            return true;
        else
            return false;
    }

}