using System.Collections;
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
    [SerializeField] GameObject canvas;

    private List<Card> Deck = new List<Card>();
    private List<Card> UnshuffledDeck = new List<Card>();
    private List<Card> Ground = new List<Card>();
    private List<List<Card>> Playground = new List<List<Card>>();
    private static List<Move> Moves = new List<Move>();
    private static List<Move> Helps = new List<Move>();

    private const string BACK_OF_A_CARD_SPRITE_NAME = "Red Back of a card";
    private const string EMPTY_DECK_SPRTIE_NAME = "Blue Back of a card";

    private int remainingRefreshes;

    public static int moveCount;
    public static bool isGameOver;


    private void Start()
    {
        moveCount = 0;
        isGameOver = false;
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


            int cardSuitIndex = (int)(card.ID / 13f);
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
                cardObj.GetComponent<CardMoveControl>().isDeckCard = false;
                cardObj.GetComponent<CardMoveControl>().isPlayable = false;
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
            cardObj.GetComponent<CardMoveControl>().isDeckCard = true;
            cardObj.GetComponent<CardMoveControl>().isPlayable = true;
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
                int groundCardCount = groundObj.transform.childCount;
                int cantPlayCount = 0;
                for (int i = 0; i < groundCardCount; i++)
                {
                    var topCard = groundObj.transform.GetChild(i);
                    if (!topCard.GetComponent<CardMoveControl>().isPlayable)
                    {
                        cantPlayCount++;
                    }
                }
                if (cantPlayCount == groundCardCount)
                {
                    cantPlayCount = 0;
                    GameOver();
                }
                else
                    for (int i = 0; i < groundCardCount; i++)
                    {
                        var bottomCard = groundObj.transform.GetChild(0);
                        bottomCard.gameObject.SetActive(true);
                        var cardInList = Ground.Find(x => x.ImageName == bottomCard.name);
                        Ground.Remove(cardInList);
                        Deck.Add(cardInList);

                        bottomCard.transform.SetParent(deckObj.transform);
                        bottomCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                        bottomCard.GetComponent<CardMoveControl>().isFacingUp = false;
                        bottomCard.GetComponent<CardMoveControl>().isPlayable = true;
                        bottomCard.GetComponent<CardMoveControl>().isDeckCard = true;
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

    private void GameOver()
    {
        Debug.Log("GAME OVER");
    }

    public static void AddMove(Move move)
    {
        Moves.Add(move);
        moveCount++;
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

            if (origin.childCount > 0)
            {
                var lastChild = origin.GetChild(origin.childCount - 1);
                lastChild.GetComponent<CardMoveControl>().isFacingUp = false;
                if (!lastChild.CompareTag("Ground"))
                    lastChild.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
            }
            if (newTarget.name.Contains("Deck"))
            {
                card.GetComponent<CardMoveControl>().enabled = false;
                card.GetComponent<Button>().enabled = true;
            }
            card.transform.SetParent(newTarget);
            if (origin.name.Contains("Panel"))
                origin.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(origin); // set the spacing for the panel layout
            if (target.name.Contains("Panel"))
                target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(target); // set the spacing for the panel layout

            LayoutRebuilder.ForceRebuildLayoutImmediate(card.transform.parent.GetComponent<RectTransform>()); // refresh layout

            Moves.Remove(move);
        }
    }
    public List<Step> ShowNextStep()
    {
        int playcardsPanelCount = playcardsObj.transform.childCount;

        // From playground cards = origin
        for (int i = 0; i < playcardsPanelCount; i++)
        {
            var originPanel = playcardsObj.transform.GetChild(i);
            int originPanelCardCount = originPanel.transform.childCount;
            for (int k = 0; k < originPanelCardCount; k++)
            {
                var originCard = originPanel.GetChild(k);
                if (originCard.GetComponent<CardMoveControl>().isFacingUp)
                {
                    if (originPanel.childCount - 1 == k)  // the card must be the top card of the panel
                    {
                        var result = ToAcePanel(originCard);
                        if (!IsNullOrEmpty(result))
                            return result;
                    }

                    for (int j = 0; j < playcardsPanelCount; j++)
                    {
                        var comparePanel = playcardsObj.transform.GetChild(j);
                        var result = ToPlayground(originCard, comparePanel, originPanel);
                        if (!IsNullOrEmpty(result))
                            return result;
                    }
                }
            }
        }

        // From ground card = origin
        if (groundObj.transform.childCount > 0)
        {
            int topGroundCardIndex = groundObj.transform.childCount - 1;
            var groundPanel = groundObj.transform;
            var groundCard = groundObj.transform.GetChild(topGroundCardIndex);

            var aceResult = ToAcePanel(groundCard);
            if (!IsNullOrEmpty(aceResult))
                return aceResult;

            for (int j = 0; j < playcardsPanelCount; j++)
            {
                var playcardsPanel = playcardsObj.transform.GetChild(j);

                var result = ToPlayground(groundCard, playcardsPanel, groundPanel);
                if (!IsNullOrEmpty(result))
                    return result;
                else if (groundCard.GetComponent<CardMoveControl>().isDeckCard)
                    groundCard.GetComponent<CardMoveControl>().isPlayable = false;

            }
        }
        List<Step> empty = null;
        return empty;
    }
    public static bool IsNullOrEmpty(ICollection collection)
    {
        return collection == null || collection.Count == 0;
    }

    private List<Step> ToPlayground(Transform originCard, Transform targetPanel, Transform originPanel = null)
    {
        if (originPanel != targetPanel)
        {
            int originPanelCardCount = 1;
            int originCardSiblingIndex = 0;
            if (originPanel != null)
            {
                originPanelCardCount = originPanel.childCount;
                originCardSiblingIndex = originCard.GetSiblingIndex();
            }
            char originCardSuit = originCard.name[0];
            int originCardValue = int.Parse(originCard.name.Substring(1));

            int targetPanelCardCount = targetPanel.childCount;
            if (targetPanelCardCount > 0)
            {
                for (int i = 0; i < targetPanelCardCount; i++)
                {
                    var targetCard = targetPanel.GetChild(i);
                    if (i == targetPanel.childCount - 1) // target card must be on top
                    {
                        if (targetCard.GetComponent<CardMoveControl>().isFacingUp)
                        {
                            char targetCardSuit = targetCard.name[0];
                            int targetCardValue = int.Parse(targetCard.name.Substring(1));
                            if (originCardValue == targetCardValue - 1 && IsPlaygroudCardSuitTrue(targetCardSuit, originCardSuit))
                            {
                                List<Step> steps = new List<Step>();
                                for (int z = originPanelCardCount - 1; z >= originCardSiblingIndex; z--)
                                {
                                    Move move = new Move();
                                    move.Card = originCard.gameObject;
                                    move.Origin = originPanel;
                                    move.Target = targetPanel;
                                    if (Helps.Count > 0 && move != null)
                                    {
                                        if (!Helps.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
                                        {
                                            Step step = new Step();
                                            step.Card = move.Card;
                                            step.Target = move.Target;
                                            steps.Add(step);
                                        }
                                    }
                                    else if (Helps.Count == 0)
                                    {
                                        Step step = new Step();
                                        step.Card = move.Card;
                                        step.Target = move.Target;
                                        steps.Add(step);
                                    }
                                }
                                return steps;
                            }
                        }
                    }
                }
            }
            else if (targetPanelCardCount == 0 && originCardValue == 13)
            {
                List<Step> steps = new List<Step>();

                for (int y = originCardSiblingIndex; y < originPanelCardCount; y++)
                {
                    Move move = new Move();
                    move.Card = originCard.gameObject;
                    move.Origin = originPanel;
                    move.Target = targetPanel;
                    if (!Helps.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
                    {
                        Step step = new Step();
                        step.Card = move.Card;
                        step.Target = move.Target;
                        steps.Add(step);
                    }
                }
                return steps;
            }
        }

        List<Step> empty = null;
        return empty;
    }

    private List<Step> ToAcePanel(Transform firstCard)
    {
        char firstCardSuit = firstCard.name[0];
        int firstCardValue = int.Parse(firstCard.name.Substring(1));
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
            else if (firstCardValue == 1 && acesPanel.childCount == 0)
            {
                Step step = new Step();
                step.Card = firstCard.gameObject;
                step.Target = acesPanel;
                return step.ToList();
            }
        }
        List<Step> empty = null;
        return empty;
    }

    public void Help()
    {
        var steps = ShowNextStep();
        if (IsNullOrEmpty(steps))
        {
            VibrateDeck();
            //if (deckObj.transform.childCount > 0)
            //{
            //    deckObj.transform.GetChild(deckObj.transform.childCount - 1).GetComponent<Button>().onClick.Invoke();
            //}
            //else if (deckObj.transform.childCount == 0)
            //{
            //    deckObj.transform.GetComponent<Button>().onClick.Invoke();
            //}
        }
        else
        {
            steps.Reverse();
            foreach (var item in steps)
            {
                Move move = new Move();
                move.Card = item.Card;
                move.Origin = item.Card.transform.parent;
                move.Target = item.Target;
                if (!Helps.Contains(move))
                {
                    // move (fly) the object to wnated position and destroy it
                    StartCoroutine(SlideCard(move));

                    Helps.Add(move);
                    //AutoMove(item, move);
                    //AddMove(move);
                }
            }
        }
    }



    private void VibrateDeck()
    {
        StartCoroutine(Shake(deckObj));
    }
    private IEnumerator Shake(GameObject dp)
    {
        float s = 0.05f;
        float seconds;
        float t = 0f;
        var dpPos = dp.GetComponent<RectTransform>().position;
        Vector3 left = new Vector3(1000f, dpPos.y, dpPos.z);
        Vector3 right = new Vector3(1010f, dpPos.y, dpPos.z);
        for (int i = 0; i < 5; i++)
        {
            seconds = s;
            t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                dp.GetComponent<RectTransform>().position = Vector3.Lerp(dp.GetComponent<RectTransform>().position, left, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            seconds = s;
            t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                dp.GetComponent<RectTransform>().position = Vector3.Lerp(dp.GetComponent<RectTransform>().position, right, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }
        seconds = s;
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            dp.GetComponent<RectTransform>().position = Vector3.Lerp(dp.GetComponent<RectTransform>().position, dpPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
    private IEnumerator SlideCard(Move move)
    {
        var positionDummy = Instantiate(cardPrefab) as GameObject;
        positionDummy.transform.SetParent(move.Target);
        move.Target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(move.Target); // set the spacing for the panel layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(positionDummy.transform.parent.GetComponent<RectTransform>()); // refresh layout
        var pos = positionDummy.transform.position;
        Destroy(positionDummy);
        var movingDummy = Instantiate(cardPrefab) as GameObject;
        movingDummy.transform.position = move.Card.transform.position;
        movingDummy.transform.SetParent(canvas.transform);
        movingDummy.GetComponent<Image>().sprite = move.Card.GetComponent<Image>().sprite;
        yield return StartCoroutine(SlideEffect(movingDummy, pos));
        yield return StartCoroutine(Disappear(movingDummy));

        move.Target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(move.Target); // set the spacing for the panel layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(move.Target.GetComponent<RectTransform>()); // refresh layout     
    }

    private IEnumerator SlideEffect(GameObject movingDummy, Vector3 pos)
    {
        float seconds = 1f;
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            movingDummy.transform.position = Vector3.Lerp(movingDummy.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }

    private IEnumerator Disappear(GameObject dp)
    {
        float seconds = 0.3f;
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            dp.GetComponent<RectTransform>().localScale = Vector3.Lerp(dp.GetComponent<RectTransform>().localScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        Destroy(dp);
    }

    private void AutoMove(Step step, Move move)
    {
        step.Card.transform.SetParent(step.Target);

        int groundCardCount = groundObj.transform.childCount;
        for (int i = 0; i < groundCardCount; i++)
        {
            groundObj.transform.GetChild(i).GetComponent<CardMoveControl>().isPlayable = true;
        }

        if (move.Origin.name.Contains("Panel"))
            move.Origin.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(move.Origin); // set the spacing for the panel layout
        if (move.Target.name.Contains("Panel"))
            move.Target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(move.Target); // set the spacing for the panel layout

        LayoutRebuilder.ForceRebuildLayoutImmediate(move.Card.transform.parent.GetComponent<RectTransform>()); // refresh layout
        if (move.Origin.childCount > 0)
        {
            var originCurrentLastCard = move.Origin.GetChild(move.Origin.childCount - 1);
            originCurrentLastCard.GetComponent<CardMoveControl>().isFacingUp = true;
            originCurrentLastCard.GetComponent<CardMoveControl>().enabled = true;
            originCurrentLastCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(originCurrentLastCard.name);
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