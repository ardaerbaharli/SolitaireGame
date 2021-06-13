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
    public bool isGameOver;
    public bool didPlayerWin;
    public bool isCelebrated;

    private void Awake()
    {
        moveCount = 0;
        isGameOver = false;
        // FOR TESTING 
        if (Settings.drawingCardCount == 0)
        {
            Settings.drawingCardCount = 1;
            Settings.deckRefreshCount = 999;
        }
        //
        remainingRefreshes = Settings.deckRefreshCount;
        UnshuffledDeck = CreateADeck();
        DealPlayCards();
        DealDeck();
    }
    private void Update()
    {
        if (!isGameOver)
        {
            int deckCardCount = deckObj.transform.childCount;
            if (deckCardCount == 0)
            {
                int groundCardCount = groundObj.transform.childCount;
                if (groundCardCount == 0)
                {
                    bool isIt = true;
                    for (int i = 0; i < playcardsObj.transform.childCount; i++)
                    {
                        int playcardsPanelICardCount = playcardsObj.transform.GetChild(i).childCount;
                        if (playcardsPanelICardCount != 0)
                            isIt = false;
                    }
                    if (isIt)
                    {
                        isGameOver = true;
                        didPlayerWin = true;
                    }
                    //bool isAllCardsAreFacingUp = true;
                    //for (int i = 0; i < playcardsObj.transform.childCount; i++)
                    //{
                    //    var playcardsPaneli = playcardsObj.transform.GetChild(i);
                    //    int playcardsPanelJCardCount = playcardsPaneli.childCount;

                    //    for (int j = 0; j < playcardsPanelJCardCount; j++)
                    //    {
                    //        var card = playcardsPaneli.GetChild(j);
                    //        if (!card.GetComponent<CardMoveControl>().isFacingUp)
                    //            isAllCardsAreFacingUp = false;
                    //    }
                    //}
                    //if (isAllCardsAreFacingUp)
                    //{
                    //    //   StartCoroutine(WinCardMove());
                    //    isGameOver = true;
                    //    didPlayerWin = true;
                    //}
                }
            }
        }
        else if (isGameOver && !isCelebrated)
        {
            if (didPlayerWin)
            {
                for (int i = 0; i < acePanel.transform.childCount; i++)
                {
                    var acePanelI = acePanel.transform.GetChild(i);

                    for (int j = 0; j < acePanelI.childCount; j++)
                    {
                        var card = acePanelI.GetChild(j);
                        card.GetComponent<CardControl>().enabled = true;
                        var randomVector = GetRandomVector();
                        card.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                        card.GetComponent<Rigidbody2D>().gravityScale = 0;
                        card.GetComponent<Rigidbody2D>().mass = 0;
                        card.GetComponent<Rigidbody2D>().AddForce(randomVector, ForceMode2D.Impulse);
                    }
                }
                isCelebrated = true;
                StartCoroutine(DestroyAllCards());
                GameOver();
            }
            else if (!didPlayerWin)
            {
                Debug.Log("You lost");
                GameOver();
            }
        }
    }

    public void WinCardMoveButton()
    {
        StartCoroutine(WinCardMove());
    }
    private IEnumerator WinCardMove()
    {
        possibleMoves = new List<List<Move>>();
        var moves = ShowNextMove();
        while (!IsNullOrEmpty(moves))
        {
            foreach (var move in moves)
            {
                if (move.Target.childCount == 0)
                {
                    if (move.Target.name.Contains("Panel"))
                    {
                        int cardValue = int.Parse(move.Card.name.Substring(1));
                        if (cardValue == 13 && !move.Card.GetComponent<CardControl>().didGoToEmptySpot)
                            possibleMoves.Add(moves);
                    }
                    else
                        possibleMoves.Add(moves);
                }
                else
                    possibleMoves.Add(moves);
            }
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
            DealFromDeck();
        }
        else
        {
            var helpMoves = new List<Move>();
            foreach (var move in moves)
            {
                if (move.Target.childCount == 0)
                {
                    int cardValue = int.Parse(move.Card.name.Substring(1));
                    if (cardValue == 13 && move.Card.GetComponent<CardControl>().didGoToEmptySpot)
                    {
                        move.Card.GetComponent<CardControl>().isK = true;
                        move.Card.GetComponent<CardControl>().didGoToEmptySpot = true;
                    }
                }

                helpMoves.Add(move);
                HelpHistory.Add(move);
            }

            StartCoroutine(AutoMove(helpMoves));
        }
        yield return null;
    }

    private IEnumerator DestroyAllCards()
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < acePanel.transform.childCount; i++)
        {
            var acePanelI = acePanel.transform.GetChild(i);

            for (int j = 0; j < acePanelI.childCount; j++)
            {
                Destroy(acePanelI.GetChild(j).gameObject);
            }
        }
    }
    private Vector2 GetRandomVector()
    {
        float x = Random.Range(-1000f, 1000f);
        float y = Random.Range(-1000f, 1000f);
        if (x < 300 && x > -300)
            return GetRandomVector();
        if (y < 300 && y > -300)
            return GetRandomVector();
        var vector = new Vector2(x, y);
        return vector;
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
                cardObj.GetComponent<CardControl>().isDeckCard = false;
                cardObj.GetComponent<CardControl>().isPlayable = false;
                if (column == piece + 1)
                {
                    cardObj.GetComponent<CardControl>().enabled = true;
                    cardObj.GetComponent<CardControl>().isFacingUp = true;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.ImageName);
                }
                else
                {
                    cardObj.GetComponent<CardControl>().enabled = false;
                    cardObj.GetComponent<CardControl>().isFacingUp = false;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                }
                if (card.Value == 13)
                {
                    cardObj.GetComponent<CardControl>().isK = true;
                    cardObj.GetComponent<CardControl>().didGoToEmptySpot = false;
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
            cardObj.GetComponent<CardControl>().isFacingUp = false;
            cardObj.GetComponent<CardControl>().enabled = false;
            cardObj.GetComponent<CardControl>().isDeckCard = true;
            cardObj.GetComponent<CardControl>().isPlayable = true;
            cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
            cardObj.AddComponent<Button>();
            cardObj.GetComponent<Button>().onClick.AddListener(delegate { DealFromDeck(); });
            if (card.Value == 13)
            {
                cardObj.GetComponent<CardControl>().isK = true;
                cardObj.GetComponent<CardControl>().didGoToEmptySpot = false;
            }
        }
    }
    public bool DealFromDeck()
    {
        int cardsToDeal = Settings.drawingCardCount;
        for (int i = 0; i < cardsToDeal; i++)
        {
            if (deckObj.transform.childCount > 0)
            {
                int lastCardInDeckIndex = deckObj.transform.childCount - 1;
                var topCard = deckObj.transform.GetChild(lastCardInDeckIndex);
                topCard.transform.SetParent(groundObj.transform);
                topCard.GetComponent<Image>().sprite = Resources.Load<Sprite>(topCard.name);
                topCard.GetComponent<CardControl>().enabled = true;
                topCard.GetComponent<CardControl>().isFacingUp = true;
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
            else
                return false;
        }
        return true;
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
                    if (!topCard.GetComponent<CardControl>().isPlayable)
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
                        bottomCard.GetComponent<CardControl>().isFacingUp = false;
                        bottomCard.GetComponent<CardControl>().isPlayable = true;
                        bottomCard.GetComponent<CardControl>().isDeckCard = true;
                        bottomCard.GetComponent<CardControl>().enabled = false;
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
                        if (!lastChildOfNewTarget.GetComponent<CardControl>().wasFacingUp)
                        {
                            lastChildOfNewTarget.GetComponent<CardControl>().isFacingUp = false;
                            lastChildOfNewTarget.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                        }
                    }
                    else if (newTarget.CompareTag("Ground"))
                    {
                        var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 3);
                        lastChildOfNewTarget.GetComponent<CardControl>().isFacingUp = false;
                        lastChildOfNewTarget.gameObject.SetActive(false);
                    }
                    else if (newTarget.name.Contains("Deck"))
                    {
                        var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 1);
                        lastChildOfNewTarget.GetComponent<CardControl>().isFacingUp = false;
                        lastChildOfNewTarget.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);

                        card.GetComponent<CardControl>().isFacingUp = false;
                        card.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
                    }
                }
                if (newTarget.name.Contains("Deck"))
                {
                    card.GetComponent<CardControl>().enabled = false;
                    card.GetComponent<Button>().enabled = true;
                }
                card.transform.SetParent(newTarget);
                if (origin.name.Contains("Panel"))
                    origin.transform.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(origin); // set the spacing for the panel layout
                if (target.name.Contains("Panel"))
                    target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(target); // set the spacing for the panel layout

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

            StartCoroutine(SlideCard(helpMoves, true));
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
                if (originCard.GetComponent<CardControl>().isFacingUp)
                {

                    for (int j = 0; j < playcardsPanelCount; j++)
                    {
                        var comparePanel = playcardsObj.transform.GetChild(j);
                        var result = ToPlayground(originCard, comparePanel, originPanel);
                        if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result) && !IsItDumbMove(result))
                        {
                            return result;
                        }
                    }

                    if (originPanel.childCount - 1 == k)  // the card must be the top card of the panel
                    {
                        var result = ToAcePanel(originCard);
                        if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result) && !IsItDumbMove(result))
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
                if (!IsNullOrEmpty(result) && !DoesContains(possibleMoves, result) && !IsItDumbMove(result))
                {
                    return result;
                }
                else if (groundCard.GetComponent<CardControl>().isDeckCard)
                    groundCard.GetComponent<CardControl>().isPlayable = false;
            }

            var aceResult = ToAcePanel(groundCard);
            if (!IsNullOrEmpty(aceResult) && !DoesContains(possibleMoves, aceResult) && !IsItDumbMove(aceResult))
            {
                return aceResult;
            }
        }
        List<Move> empty = null;
        return empty;
    }

    private bool IsItDumbMove(List<Move> result)
    {
        var origin = result.First().Origin;
        var target = result.First().Target;

        if (origin.childCount == 1) // for the card K => if it is already in the empty panel, it doesnt need to change position
        {
            if (target.childCount == 0)
                return true;
            else
                return false;
        }
        else if (origin.childCount >= 2 && target.childCount >= 1)
        {
            var bigBrotherOfOriginCardIndex = result.First().Card.transform.GetSiblingIndex() - 1;
            int bigBrotherOfOriginCardValue;
            if (bigBrotherOfOriginCardIndex > 1)
            {
                var bigBrotherOfOriginCard = origin.GetChild(bigBrotherOfOriginCardIndex);
                bigBrotherOfOriginCardValue = int.Parse(bigBrotherOfOriginCard.name.Substring(1));
            }
            else
            {
                bigBrotherOfOriginCardValue = -1;
            }
            var targetCard = target.GetChild(target.childCount - 1);
            int targetCardValue = int.Parse(targetCard.name.Substring(1));

            if (bigBrotherOfOriginCardValue == targetCardValue)
                return true;
            else
                return false;
        }
        else
            return false;
        ////var bigBrotherOfOriginCardIndex = result.First().Card.transform.GetSiblingIndex() - 1;

        ////var bigBrotherOfOriginCard = origin.GetChild(bigBrotherOfOriginCardIndex);
        ////var bigBrotherOfOriginCardValue = int.Parse(bigBrotherOfOriginCard.name.Substring(1));

        //if (target.childCount > 0)
        //{
        //    var targetCard = target.GetChild(target.childCount - 1);
        //    int targetCardValue = int.Parse(targetCard.name.Substring(1));
        //    //int a = int.Parse(result.First().Origin.GetChild(result.First().Card.transform.GetSiblingIndex()).name.Substring(1));
        //    if (bigBrotherOfOriginCardValue == targetCardValue)
        //        return true;
        //    else
        //        return false;
        //}
        //else if (target.childCount == 0)
        //{
        //    if (bigBrotherOfOriginCardIndex == 0)
        //        return true;
        //    else
        //        return false;
        //}
        //return false;
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
                        if (targetCard.GetComponent<CardControl>().isFacingUp && !targetCard.GetComponent<CardControl>().isDummy)
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
            else if (targetPanelCardCount == 0 && originCardValue == 13 && !originCard.GetComponent<CardControl>().didGoToEmptySpot)
            {
                var moves = new List<Move>();
                //originCard.GetComponent<CardMoveControl>().isK = true;
                //originCard.GetComponent<CardMoveControl>().didGoToEmptySpot = true;
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
                    if (!HelpHistory.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
                        return move.ToList();
                }
            }
            else if (firstCardValue == 1 && acesPanel.childCount == 0)
            {
                var move = new Move();
                move.Card = firstCard.gameObject;
                move.Target = acesPanel;
                move.Origin = move.Card.transform.parent;
                if (!HelpHistory.Any(x => x.Card.name == move.Card.name && x.Origin.name == move.Origin.name && x.Target.name == move.Target.name))
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
    private IEnumerator SlideCard(List<Move> helpMoves, bool willDisappear)
    {
        var target = helpMoves.First().Target;
        var cardPositions = new List<Vector3>();
        var posDummies = new List<GameObject>();
        foreach (var move in helpMoves)
        {
            var positionDummy = Instantiate(cardPrefab) as GameObject;
            positionDummy.GetComponent<CardControl>().isDummy = true;
            positionDummy.transform.SetParent(move.Target);
            positionDummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            cardPositions.Add(move.Card.transform.position);
            posDummies.Add(positionDummy);
        }

        if (!target.parent.CompareTag("AcesPanel"))
        {
            target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(target); // set the spacing for the panel layout
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout

        var positions = new List<Vector3>();
        foreach (var positionDummy in posDummies)
        {
            var pos = positionDummy.transform.position;
            positions.Add(pos);
        }

        var movingDummies = new List<GameObject>();

        for (int i = 0; i < positions.Count; i++)
        {
            var movingDummy = Instantiate(cardPrefab) as GameObject;
            movingDummy.GetComponent<CardControl>().isDummy = true;
            movingDummy.transform.position = cardPositions[i];
            movingDummy.transform.SetParent(canvas.transform);
            movingDummy.GetComponent<Image>().sprite = helpMoves[i].Card.GetComponent<Image>().sprite;
            movingDummies.Add(movingDummy);
        }
        for (int i = 0; i < movingDummies.Count; i++)
        {
            StartCoroutine(SlideAndDestroy(movingDummies[i], positions[i], willDisappear));
        }

        foreach (var positionDummy in posDummies)
        {
            DestroyImmediate(positionDummy);
        }

        if (!target.parent.CompareTag("AcesPanel"))
        {
            target.transform.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(target); // set the spacing for the panel layout
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>()); // refresh layout
        yield return null;
    }
    private IEnumerator SlideAndDestroy(GameObject movingDummy, Vector3 pos, bool willDisappear)
    {
        float seconds = 1f;
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            movingDummy.transform.position = Vector3.Lerp(movingDummy.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        if (willDisappear)
        {
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
    }
    private IEnumerator AutoMove(List<Move> moves)
    {
        //  yield return StartCoroutine(SlideCard(moves, false));
        AddMove(moves);
        foreach (var move in moves)
        {
            var originParent = move.Origin;
            var targetParent = move.Target;
            var card = move.Card;

            if (move.Target.name.Contains("Panel"))
            {
                if (originParent.CompareTag("Ground") || originParent.CompareTag("AcePlaceholderPanel"))
                {
                    card.transform.SetParent(targetParent);

                    targetParent.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(targetParent); ; // set the spacing for the panel layout                 

                    if (originParent.CompareTag("Ground"))
                    {
                        card.transform.GetComponent<CardControl>().isDeckCard = false;

                        int groundCardCount = originParent.childCount;
                        for (int i = 0; i < groundCardCount; i++)
                        {
                            groundObj.transform.GetChild(i).GetComponent<CardControl>().isPlayable = true;
                        }

                        if (originParent.transform.childCount > 2)
                        {
                            int index = originParent.transform.childCount - 1 - 2;
                            originParent.transform.GetChild(index).gameObject.SetActive(true);
                        }
                    }
                }
                else if (originParent.CompareTag("PlaycardsPanelChildren"))
                {
                    card.transform.SetParent(targetParent);
                    if (originParent.childCount > 0)
                    {
                        // change the image and enable the cardmovecontrol script
                        var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                        lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                        lastChildOfTheOriginParent.GetComponent<CardControl>().enabled = true;
                        lastChildOfTheOriginParent.GetComponent<CardControl>().isFacingUp = true;
                    }

                    originParent.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(originParent);  // set the spacing for the panel layout
                    targetParent.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(targetParent);  // set the spacing for the panel layout
                }
            }
            else if (move.Target.parent.name.Contains("AcesPanel"))
            {
                card.transform.SetParent(targetParent); // change the parent of the card              

                if (originParent.CompareTag("PlaycardsPanelChildren"))
                {
                    if (originParent.childCount > 0)
                    {
                        // change the image and enable the cardmovecontrol script
                        var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                        lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                        lastChildOfTheOriginParent.GetComponent<CardControl>().enabled = true;
                        lastChildOfTheOriginParent.GetComponent<CardControl>().isFacingUp = true;
                    }
                    originParent.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(originParent); // set the spacing for the panel layout
                }
                else if (originParent.CompareTag("Ground")) // 6  => 1 2 (3 4 5 )
                {
                    int groundCardCount = originParent.childCount;
                    for (int i = 0; i < groundCardCount; i++)
                    {
                        groundObj.transform.GetChild(i).GetComponent<CardControl>().isPlayable = true;
                    }

                    if (originParent.transform.childCount > 2)
                    {
                        int index = originParent.transform.childCount - 1 - 2;
                        originParent.transform.GetChild(index).gameObject.SetActive(true);
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(card.transform.parent.GetComponent<RectTransform>()); // refresh layout
        }
        yield return null;
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