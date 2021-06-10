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

    private List<Card> UnshuffledDeck = new List<Card>();
    private static List<List<Move>> Moves = new List<List<Move>>();
    private static List<Move> HelpHistory = new List<Move>();

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
            for (int piece = 0; piece < column; piece++)
            {
                var columnIndex = column - 1;
                var card = Draw();

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
                if (card.Value == 13)
                {
                    cardObj.GetComponent<CardMoveControl>().isK = true;
                    cardObj.GetComponent<CardMoveControl>().didGoToEmptySpot = false;
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
            if (card.Value == 13)
            {
                cardObj.GetComponent<CardMoveControl>().isK = true;
                cardObj.GetComponent<CardMoveControl>().didGoToEmptySpot = false;
            }
        }
    }
    private void DealFromDeck()
    {
        int cardsToDeal = Settings.drawingCardCount;
        for (int i = 0; i < cardsToDeal; i++)
        {
            int lastCardInDeckIndex = deckObj.transform.childCount - 1;
            var topCard = deckObj.transform.GetChild(lastCardInDeckIndex);


            topCard.transform.SetParent(groundObj.transform);
            topCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(topCard.name);
            topCard.GetComponent<CardMoveControl>().enabled = true;
            topCard.GetComponent<CardMoveControl>().isFacingUp = true;
            topCard.GetComponent<Button>().enabled = false;

            Move move = new Move();
            move.Origin = deckObj.transform;
            move.Card = topCard.gameObject;
            move.Target = groundObj.transform;
            AddMove(move.ToList());

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


    public static void AddMove(List<Move> move)
    {
        Moves.Add(move);
        moveCount++;
    }
    public void Undo()
    {
        var move = Moves.Last();
        if (move != null)
        {
            foreach (var step in move)
            {
                var card = step.Card;
                var origin = step.Origin;
                var target = step.Target;
                var newTarget = origin;

                if (newTarget.childCount > 0)
                {
                    if (newTarget.name.Contains("Panel"))
                    {
                        var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 1);
                        if (!lastChildOfNewTarget.GetComponent<CardMoveControl>().wasFacingUp)
                        {
                            lastChildOfNewTarget.GetComponent<CardMoveControl>().isFacingUp = false;
                            lastChildOfNewTarget.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                        }
                    }
                    else if (newTarget.CompareTag("Ground"))
                    {
                        var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 3);
                        lastChildOfNewTarget.GetComponent<CardMoveControl>().isFacingUp = false;
                        lastChildOfNewTarget.gameObject.SetActive(false);
                    }
                    else if (newTarget.name.Contains("Deck"))
                    {
                        var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 1);
                        lastChildOfNewTarget.GetComponent<CardMoveControl>().isFacingUp = false;
                        lastChildOfNewTarget.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);

                        card.GetComponent<CardMoveControl>().isFacingUp = false;
                        card.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                    }
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
    }

    List<List<Move>> possibleMoves = new List<List<Move>>();

    public void Help()
    {
        possibleMoves = new List<List<Move>>();
        var moves = ShowNextMove();
        while (!IsNullOrEmpty(moves))
        {
            possibleMoves.Add(moves);
            moves = ShowNextMove();
        }
        if (possibleMoves.Count > 1)
        {
            possibleMoves.Sort((a, b) => a.Count - b.Count);
            moves = possibleMoves.Last();
        }
        else
        {
            moves = possibleMoves.FirstOrDefault();
        }

        if (IsNullOrEmpty(moves))
        {
            VibrateDeck();
        }
        else
        {
            var helpMoves = new List<Move>();
            foreach (var item in moves)
            {
                Move move = new Move();
                move.Card = item.Card;
                move.Origin = item.Card.transform.parent;
                move.Target = item.Target;
                helpMoves.Add(move);
                HelpHistory.Add(move);
            }


            StartCoroutine(SlideCard(helpMoves));
        }
    }

    public List<Move> ShowNextMove()
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

                    for (int j = 0; j < playcardsPanelCount; j++)
                    {
                        var comparePanel = playcardsObj.transform.GetChild(j);
                        var result = ToPlayground(originCard, comparePanel, originPanel);
                        if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result))
                        {
                            return result;
                        }
                    }

                    if (originPanel.childCount - 1 == k)  // the card must be the top card of the panel
                    {
                        var result = ToAcePanel(originCard);
                        if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result))
                        {
                            return result;
                        }
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

            for (int j = 0; j < playcardsPanelCount; j++)
            {
                var playcardsPanel = playcardsObj.transform.GetChild(j);

                var result = ToPlayground(groundCard, playcardsPanel, groundPanel);
                if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result))
                {
                    return result;
                }
                else if (groundCard.GetComponent<CardMoveControl>().isDeckCard)
                    groundCard.GetComponent<CardMoveControl>().isPlayable = false;
            }


            var aceResult = ToAcePanel(groundCard);
            if (!IsNullOrEmpty(aceResult) && !DoesContains(possibleMoves, aceResult))
            {
                return aceResult;
            }
        }
        List<Move> empty = null;
        return empty;
    }

    private bool DoesContains(List<List<Move>> possibleMoves, List<Move> move)
    {
        for (int i = 0; i < possibleMoves.Count; i++)
        {
            foreach (var possibleMove in possibleMoves[i])
            {
                for (int k = 0; k < move.Count; k++)
                {
                    if (possibleMove.Card.name == move[k].Card.name && possibleMove.Target.name == move[k].Target.name && possibleMove.Origin.name == move[k].Origin.name)
                        return true;
                }
            }
        }
        return false;
    }

    public static bool IsNullOrEmpty(ICollection collection)
    {
        return collection == null || collection.Count == 0;
    }

    private List<Move> ToPlayground(Transform originCard, Transform targetPanel, Transform originPanel = null)
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

                        if (targetCard.GetComponent<CardMoveControl>().isFacingUp && !targetCard.GetComponent<CardMoveControl>().isDummy)
                        {
                            char targetCardSuit = targetCard.name[0];
                            int targetCardValue = int.Parse(targetCard.name.Substring(1));
                            if (originCardValue == targetCardValue - 1 && IsPlaygroudCardSuitTrue(targetCardSuit, originCardSuit))
                            {
                                var moves = new List<Move>();
                                for (int z = originCardSiblingIndex; z < originPanelCardCount; z++)
                                {
                                    Move move = new Move();
                                    move.Card = originPanel.GetChild(z).gameObject;
                                    move.Origin = originPanel;
                                    move.Target = targetPanel;
                                    if (HelpHistory.Count > 0 && move != null)
                                    {
                                        if (!HelpHistory.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
                                        {
                                            moves.Add(move);
                                        }
                                    }
                                    else if (HelpHistory.Count == 0)
                                    {
                                        moves.Add(move);
                                    }
                                }
                                return moves;
                            }
                        }
                    }
                }
            }
            else if (targetPanelCardCount == 0 && originCardValue == 13 && !originCard.GetComponent<CardMoveControl>().didGoToEmptySpot)
            {
                var moves = new List<Move>();
                originCard.GetComponent<CardMoveControl>().isK = true;
                originCard.GetComponent<CardMoveControl>().didGoToEmptySpot = true;
                for (int y = originCardSiblingIndex; y < originPanelCardCount; y++)
                {
                    Move move = new Move();
                    move.Card = originPanel.GetChild(y).gameObject;
                    move.Origin = originPanel;
                    move.Target = targetPanel;
                    if (!HelpHistory.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
                    {
                        moves.Add(move);
                    }
                }
                return moves;
            }
        }

        List<Move> empty = null;
        return empty;
    }

    private List<Move> ToAcePanel(Transform firstCard)
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
                    var move = new Move();
                    move.Card = firstCard.gameObject;
                    move.Target = acesPanel;
                    move.Origin = move.Card.transform.parent;
                    return move.ToList();
                }
            }
            else if (firstCardValue == 1 && acesPanel.childCount == 0)
            {
                var move = new Move();
                move.Card = firstCard.gameObject;
                move.Target = acesPanel;
                move.Origin = move.Card.transform.parent;
                return move.ToList();

            }
        }
        List<Move> empty = null;
        return empty;
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
    private IEnumerator SlideCard(List<Move> helpMoves)
    {
        var target = helpMoves.First().Target;
        var cardPositions = new List<Vector3>();
        var posDummies = new List<GameObject>();
        foreach (var move in helpMoves)
        {
            var positionDummy = Instantiate(cardPrefab) as GameObject;
            positionDummy.GetComponent<CardMoveControl>().isDummy = true;
            positionDummy.transform.SetParent(move.Target);
            positionDummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            cardPositions.Add(move.Card.transform.position);
            posDummies.Add(positionDummy);
        }

        if (!target.parent.CompareTag("AcesPanel"))
        {
            target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(target); // set the spacing for the panel layout
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout

        var positions = new List<Vector3>();
        foreach (var positionDummy in posDummies)
        {
            var pos = positionDummy.transform.position;
            positions.Add(pos);
        }

        var movingDummies = new List<GameObject>();

        foreach (var pos in positions)
        {
            var movingDummy = Instantiate(cardPrefab) as GameObject;
            movingDummy.GetComponent<CardMoveControl>().isDummy = true;
            movingDummy.transform.position = cardPositions[positions.IndexOf(pos)];
            helpMoves[positions.IndexOf(pos)].Card.GetComponent<Image>().enabled = false;
            movingDummy.transform.SetParent(canvas.transform);
            movingDummy.GetComponent<Image>().sprite = helpMoves[positions.IndexOf(pos)].Card.GetComponent<Image>().sprite;
            movingDummies.Add(movingDummy);
        }

        for (int i = 0; i < movingDummies.Count; i++)
        {
            StartCoroutine(SlideAndDestroy(movingDummies[i], positions[i], helpMoves[i].Card));
        }
        for (int i = 0; i < movingDummies.Count; i++)
        {
            helpMoves[i].Card.GetComponent<Image>().enabled = true;

        }

        foreach (var positionDummy in posDummies)
        {
            DestroyImmediate(positionDummy);
        }

        if (!target.parent.CompareTag("AcesPanel"))
        {
            target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardMoveControl.CalculateSpacing(target); // set the spacing for the panel layout
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout
        yield return null;
    }

    private IEnumerator SlideAndDestroy(GameObject movingDummy, Vector3 pos, GameObject card)
    {
        float seconds = 1f;
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            movingDummy.transform.position = Vector3.Lerp(movingDummy.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        seconds = 0.1f;
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            movingDummy.GetComponent<RectTransform>().localScale = Vector3.Lerp(movingDummy.GetComponent<RectTransform>().localScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        Destroy(movingDummy);
    }
    private void AutoMove(Move step, Move move)
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