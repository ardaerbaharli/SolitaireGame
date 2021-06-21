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
    [SerializeField] GameObject questionPrefab;
    [SerializeField] [Range(0, 1f)] float timeScale = 1f;
    private enum GameOverType { Win, Lose };

    private List<Card> UnshuffledDeck = new List<Card>();
    private static List<List<Move>> Moves = new List<List<Move>>();
    //private static List<Move> HelpHistory = new List<Move>();
    private List<List<Move>> possibleMoves = new List<List<Move>>();

    private const string BACK_OF_A_CARD_SPRITE_NAME = "Red Back of a card";
    private const string EMPTY_DECK_SPRTIE_NAME = "Blue Back of a card";

    private int remainingRefreshes;

    public static int moveCount;
    public bool isGameOver;
    public bool didPlayerWin;
    public bool isCelebrated;
    public bool isGameOverThingsComplete;

    private bool fillPlayground;

    private void Awake()
    {
        fillPlayground = false;
        //HelpHistory.Clear();
        Moves.Clear();
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
            Time.timeScale = timeScale;
            if (fillPlayground && Time.frameCount % 10 == 0)
            {
                var moves = Help(false);
                StartCoroutine(AutoMove(moves));
            }
            int deckCardCount = deckObj.transform.childCount;
            if (deckCardCount == 0)
            {
                int groundCardCount = groundObj.transform.childCount;
                if (groundCardCount == 0)
                {
                    bool isAllCardsInPlace = true;

                    bool isAllCardsFacingUp = true;
                    for (int i = 0; i < playcardsObj.transform.childCount; i++)
                    {
                        int playcardsPanelICardCount = playcardsObj.transform.GetChild(i).childCount;
                        var panel = playcardsObj.transform.GetChild(i);
                        for (int j = 0; j < playcardsPanelICardCount; j++)
                        {
                            if (!panel.GetChild(j).GetComponent<CardControl>().isFacingUp)
                                isAllCardsFacingUp = false;
                        }
                    }

                    if (isAllCardsFacingUp)
                    {
                        for (int i = 0; i < playcardsObj.transform.childCount; i++)
                        {
                            int playcardsPanelICardCount = playcardsObj.transform.GetChild(i).childCount;
                            if (playcardsPanelICardCount != 0)
                                isAllCardsInPlace = false;
                        }

                        if (isAllCardsInPlace)
                        {
                            didPlayerWin = true;
                            isGameOver = true;
                        }
                        else
                        {
                            fillPlayground = true;
                        }
                    }


                }
            }
        }
        else if (isGameOver && !isGameOverThingsComplete)
        {
            isGameOverThingsComplete = true;
            if (didPlayerWin)
                GameOver(GameOverType.Win);
            else if (!didPlayerWin)
                GameOver(GameOverType.Lose);
        }
    }
    private IEnumerator CelebrateWin()
    {
        yield return new WaitForSeconds(1f);
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
                //cardObj.GetComponent<Canvas>().overrideSorting = true;
                if (column == piece + 1)
                {
                    cardObj.GetComponent<CardControl>().enabled = true;
                    cardObj.GetComponent<CardControl>().isFacingUp = true;
                    cardObj.GetComponent<CardControl>().earlierLocation = gameObject.transform.position;
                    cardObj.GetComponent<CardControl>().lastKnownLocation = gameObject.transform.position;
                    cardObj.GetComponent<BoxCollider2D>().enabled = true;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.ImageName);
                }
                else
                {
                    cardObj.GetComponent<CardControl>().enabled = false;
                    cardObj.GetComponent<CardControl>().isFacingUp = false;
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
            var cardObj = Instantiate(cardPrefab) as GameObject;
            cardObj.transform.SetParent(deckObj.transform);
            cardObj.transform.position = deckObj.transform.position;
            cardObj.name = card.ImageName;
            cardObj.GetComponent<CardControl>().isFacingUp = false;
            cardObj.GetComponent<CardControl>().enabled = false;
            var v = new Vector3(0, 180f, 0);
            var q = Quaternion.Euler(v);
            cardObj.GetComponent<RectTransform>().rotation = q;
            cardObj.GetComponent<GraphicRaycaster>().ignoreReversedGraphics = false;
            cardObj.GetComponent<CardControl>().earlierLocation = gameObject.transform.position;
            cardObj.GetComponent<CardControl>().lastKnownLocation = gameObject.transform.position;

            cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
            cardObj.AddComponent<Button>();
            cardObj.GetComponent<Button>().onClick.AddListener(delegate { DealFromDeck(); });
        }
    }
    public void DealFromDeck(bool animation = true)
    {
        int dealingCardCount = Settings.drawingCardCount;
        int deckCardCount = deckObj.transform.childCount;
        if (deckCardCount < Settings.drawingCardCount)
            dealingCardCount = deckCardCount;
        int startIndex = deckCardCount - dealingCardCount;
        var moves = new List<Move>();
        var parent = groundObj.transform;

        var posDummies = new List<Transform>();
        var cardPositions = new List<Vector3>();
        for (int i = 0; i < dealingCardCount; i++)
        {
            var dummy = Instantiate(cardPrefab);
            dummy.GetComponent<CardControl>().isDummy = true;
            dummy.transform.SetParent(parent);
            dummy.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            posDummies.Add(dummy.transform);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(groundObj.GetComponent<RectTransform>()); // refresh layout

        foreach (var dummy in posDummies)
        {
            var pos = dummy.transform.position;
            cardPositions.Add(pos);
        }

        int posIndex = 0;
        for (int i = deckCardCount - 1; i >= startIndex; i--)
        {
            Destroy(posDummies[posIndex].gameObject);
            var card = deckObj.transform.GetChild(i).gameObject;
            card.GetComponent<CardControl>().enabled = true;
            card.GetComponent<BoxCollider2D>().enabled = true;
            card.GetComponent<Button>().enabled = false;

            Move move = new Move();
            move.Origin = deckObj.transform;
            move.Card = card.gameObject;
            move.Target = groundObj.transform;
            moves.Add(move);
            card.GetComponent<CardControl>().earlierLocation = gameObject.transform.position;
            card.GetComponent<CardControl>().lastKnownLocation = cardPositions[posIndex];

            StartCoroutine(SlideAndParentToGround(card, 0.5f, cardPositions[posIndex], animation));
            posIndex++;
            StartCoroutine(RotateToRevealCard(card.transform, 0.2f));
            card.GetComponent<CardControl>().isFacingUp = true;
        }

        AddMove(moves);
    }
    private IEnumerator SlideAndParentToGround(GameObject card, float time, Vector3 pos, bool animation = true)
    {
        Transform parent = groundObj.transform;
        if (animation)
        {
            float seconds = time;
            float t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                card.transform.position = Vector3.Lerp(card.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        card.transform.SetParent(parent);

        if (parent.childCount > 3)
        {
            for (int k = 0; k < parent.childCount - 3; k++)
            {
                parent.GetChild(k).gameObject.SetActive(false);
            }
        }
    }
    private IEnumerator SlideAndParent(GameObject card, Transform parent, Vector3 pos, float time = 0.3f)
    {
        card.GetComponent<Canvas>().overrideSorting = true;
        card.GetComponent<Canvas>().sortingOrder = 2;

        float seconds = time;
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            card.transform.position = Vector3.Lerp(card.transform.position, pos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        card.transform.SetParent(parent);
        card.GetComponent<Canvas>().overrideSorting = false;
    }
    private IEnumerator RotateToRevealCard(Transform card, float time = 0.2f, bool animation = true)
    {
        if (animation)
        {
            if (!card.GetComponent<CardControl>().isFacingUp)
            {
                float seconds = time;
                float t = 0f;
                var v = new Vector3(0, 90f, 0);
                var q = Quaternion.Euler(v);
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }

                card.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.name);
                card.GetComponent<CardControl>().isFacingUp = true;

                seconds = time;
                t = 0f;
                v = new Vector3(0, 0f, 0);
                q = Quaternion.Euler(v);
                while (t <= 1.0)
                {
                    t += Time.deltaTime / seconds;
                    card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
            }
        }
        else
        {
            card.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.name);
            card.GetComponent<CardControl>().isFacingUp = true;
        }
    }
    private IEnumerator RotateToHideCard(Transform card, float time = 0.2f)
    {
        float seconds = time;
        float t = 0f;
        var v = new Vector3(0, 90f, 0);
        var q = Quaternion.Euler(v);
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        card.GetComponent<Image>().sprite = Resources.Load<Sprite>(BACK_OF_A_CARD_SPRITE_NAME);
        card.GetComponent<CardControl>().isFacingUp = false;
        seconds = time;
        t = 0f;
        v = new Vector3(0, 0f, 0);
        q = Quaternion.Euler(v);
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            card.GetComponent<RectTransform>().rotation = Quaternion.Lerp(card.GetComponent<RectTransform>().rotation, q, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
    public void RefreshDeck()
    {
        if (deckObj.transform.childCount == 0)
        {
            if (remainingRefreshes > 0)
            {
                var moves = new List<Move>();
                remainingRefreshes--;
                int groundCardCount = groundObj.transform.childCount;
                //int cantPlayCount = 0;
                //for (int i = 0; i < groundCardCount; i++)
                //{
                //    var topCard = groundObj.transform.GetChild(i);
                //    if (!topCard.GetComponent<CardControl>().isPlayable)
                //    {
                //        cantPlayCount++;
                //    }
                //}
                if (!IsThereAMove())
                {
                    GameOver(GameOverType.Lose);
                }
                else
                {
                    for (int i = groundCardCount - 1; i >= 0; i--)
                    {
                        var topCard = groundObj.transform.GetChild(i);
                        topCard.gameObject.SetActive(true);

                        topCard.GetComponent<CardControl>().enabled = false;
                        topCard.GetComponent<Button>().enabled = true;

                        var m = new Move();
                        m.Card = topCard.gameObject;
                        m.Origin = topCard.parent;
                        m.Target = deckObj.transform;
                        moves.Add(m);

                        StartCoroutine(SlideAndParent(topCard.gameObject, deckObj.transform, deckObj.transform.position, 0.3f));
                        StartCoroutine(RotateToHideCard(topCard, 0.2f));
                    }
                }
            }
            else if (remainingRefreshes == 0)
            {
                deckObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(EMPTY_DECK_SPRTIE_NAME);
                GameOver(GameOverType.Lose);
            }
        }
    }
    private List<Move> ShowNextMove()
    {
        // from playground
        var moveList = FromPlayground();
        if (!IsNullOrEmpty(moveList))
            return moveList;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // from ground
        var moves = FromGround();
        if (!IsNullOrEmpty(moves))
            return moves;

        return null;
    }

    private List<Move> FromGround()
    {
        if (groundObj.transform.childCount > 0)
        {
            var card = groundObj.transform.GetChild(groundObj.transform.childCount - 1);

            if (CanItGoToAnyAcePanel(card))
            {
                var move = GetAcePanelMove(card);

                if (!IsItDumbMove(move.ToList()) && !DoesContains(possibleMoves, move.ToList()))//!DoesExistInHelpHistory(move.ToList()) &&
                    return move.ToList();
            }
            if (CanItGoToAnyPlaygroundPanel(card))
            {
                var moves = GetPlaygroundMoves(card);
                if (!IsItDumbMove(moves) && !DoesContains(possibleMoves, moves))//!DoesExistInHelpHistory(moves) && 
                    return moves;
            }
        }
        return null;
    }

    private List<Move> FromPlayground()
    {
        int playgroundPanelCount = playcardsObj.transform.childCount;

        // to ace panel
        for (int i = 0; i < playgroundPanelCount; i++)
        {
            var panel = playcardsObj.transform.GetChild(i);
            if (panel.childCount > 0)
            {
                var lastCard = panel.transform.GetChild(panel.childCount - 1);
                if (CanItGoToAnyAcePanel(lastCard))
                {
                    var move = GetAcePanelMove(lastCard);

                    if (!IsItDumbMove(move.ToList()) && !DoesContains(possibleMoves, move.ToList()))//!DoesExistInHelpHistory(move.ToList()) &&
                        return move.ToList();
                }
            }
        }

        // to playground
        for (int i = 0; i < playgroundPanelCount; i++)
        {
            var playgroundPanel = playcardsObj.transform.GetChild(i);
            int playgroundPanelCardcount = playgroundPanel.childCount;
            if (playgroundPanelCardcount > 0)
            {
                for (int k = 0; k < playgroundPanelCardcount; k++)
                {
                    var card = playgroundPanel.GetChild(k);
                    if (card.GetComponent<CardControl>().isFacingUp)
                        if (CanItGoToAnyPlaygroundPanel(card))
                        {
                            var moves = GetPlaygroundMoves(card);
                            if (!IsItDumbMove(moves) && !DoesContains(possibleMoves, moves))//!DoesExistInHelpHistory(moves) &&
                                return moves;
                        }
                }
            }
        }
        return null;
    }

    private List<Move> GetPlaygroundMoves(Transform card)
    {
        var cardSuit = card.name[0];
        int cardValue = int.Parse(card.name.Substring(1));
        int cardIndex = card.GetSiblingIndex();
        int parentChildCount = card.transform.parent.childCount;

        bool isCardK = (cardValue == 13) ? true : false;
        bool isLastCard = (parentChildCount - 1 == cardIndex) ? true : false;


        int playgroundPanelCount = playcardsObj.transform.childCount;
        for (int i = 0; i < playgroundPanelCount; i++)
        {
            var panel = playcardsObj.transform.GetChild(i);
            if (panel.childCount > 0)
            {
                var lastCard = panel.GetChild(panel.childCount - 1);
                var lastCardSuit = lastCard.name[0];
                int lastCardValue = int.Parse(lastCard.name.Substring(1));

                if (cardValue == lastCardValue - 1 && IsPlaygroudCardSuitTrue(lastCardSuit, cardSuit))
                {
                    if (isLastCard)
                    {
                        var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                        return move.ToList();
                    }
                    else
                    {
                        var moves = new List<Move>();
                        for (int k = cardIndex; k < parentChildCount; k++)
                        {
                            var move = new Move(card.transform.parent.GetChild(k).gameObject, card.transform.parent, panel.transform);
                            moves.Add(move);
                        }
                        return moves;
                    }
                }
            }
            else if (panel.childCount == 0 && isCardK)
            {
                if (isLastCard)
                {
                    var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                    return move.ToList();
                }
                else
                {
                    var moves = new List<Move>();
                    for (int k = cardIndex; k < parentChildCount; k++)
                    {
                        var move = new Move(card.gameObject, card.transform.parent, panel.transform);
                        moves.Add(move);
                    }
                    return moves;
                }
            }
        }
        return null;
    }

    private Move GetAcePanelMove(Transform card)
    {
        var cardSuit = card.name[0];
        int cardValue = int.Parse(card.name.Substring(1));

        bool isCardA = (cardValue == 1) ? true : false;

        int acePanelCount = acePanel.transform.childCount;
        for (int i = 0; i < acePanelCount; i++)
        {
            var panel = acePanel.transform.GetChild(i);
            if (panel.childCount > 0)
            {
                var lastCard = panel.GetChild(panel.childCount - 1);
                var lastCardSuit = lastCard.name[0];
                int lastCardValue = int.Parse(lastCard.name.Substring(1));
                if (lastCardSuit == cardSuit && cardValue == lastCardValue + 1)
                {
                    var move = new Move(card.gameObject, card.transform.parent, panel);
                    //move.Card = card.gameObject;
                    //move.Origin = card.transform.parent;
                    //move.Target = panel;
                    return move;
                }

            }
            else if (panel.childCount == 0 && isCardA)
            {
                var move = new Move(card.gameObject, card.transform.parent, panel);
                //move.Card = card.gameObject;
                //move.Origin = card.transform.parent;
                //move.Target = panel;
                return move;
            }
        }
        return null;
    }

    private bool IsThereAMove()
    {
        int deckCardCount = deckObj.transform.childCount;
        for (int i = 0; i < deckCardCount; i++)
        {
            var card = deckObj.transform.GetChild(i);

            if (CanItGoToAnyAcePanel(card))
                return true;
            if (CanItGoToAnyPlaygroundPanel(card))
                return true;
        }

        int groundCardCount = groundObj.transform.childCount;
        for (int i = 0; i < groundCardCount; i++)
        {
            var card = groundObj.transform.GetChild(i);

            if (CanItGoToAnyAcePanel(card))
                return true;
            if (CanItGoToAnyPlaygroundPanel(card))
                return true;
        }

        return false;
    }
    private bool CanItGoToAnyAcePanel(Transform card)
    {
        var cardSuit = card.name[0];
        int cardValue = int.Parse(card.name.Substring(1));

        bool isCardA = (cardValue == 1) ? true : false;

        int acePanelCount = acePanel.transform.childCount;
        for (int i = 0; i < acePanelCount; i++)
        {
            var panel = acePanel.transform.GetChild(i);
            if (panel.childCount > 0)
            {
                var lastCard = panel.GetChild(panel.childCount - 1);
                var lastCardSuit = lastCard.name[0];
                int lastCardValue = int.Parse(lastCard.name.Substring(1));
                if (lastCardSuit == cardSuit && cardValue == lastCardValue + 1)
                    return true;
            }
            else if (panel.childCount == 0 && isCardA)
            {
                return true;
            }
        }
        return false;
    }
    private bool CanItGoToAnyPlaygroundPanel(Transform card)
    {
        var cardSuit = card.name[0];
        int cardValue = int.Parse(card.name.Substring(1));

        bool isCardK = (cardValue == 13) ? true : false;

        int playgroundPanelCount = playcardsObj.transform.childCount;
        for (int i = 0; i < playgroundPanelCount; i++)
        {
            var panel = playcardsObj.transform.GetChild(i);
            if (panel.childCount > 0)
            {
                var lastCard = panel.GetChild(panel.childCount - 1);
                var lastCardSuit = lastCard.name[0];
                int lastCardValue = int.Parse(lastCard.name.Substring(1));

                if (cardValue == lastCardValue - 1 && IsPlaygroudCardSuitTrue(lastCardSuit, cardSuit))
                    return true;
            }
            else if (panel.childCount == 0 && isCardK)
            {
                return true;
            }
        }
        return false;
    }
    private void GameOver(GameOverType type)
    {
        isGameOver = true;

        Debug.Log("GAME OVER");

        if (type == GameOverType.Win)
        {
            Debug.Log("YOU WON");
            if (!isCelebrated)
            {
                StartCoroutine(CelebrateWin());
                StartCoroutine(DestroyAllCards());
            }
        }
        else if (type == GameOverType.Lose)
        {
            Debug.Log("YOU LOST");
        }
    }
    public static void AddMove(List<Move> move)
    {
        Moves.Add(move);
        helpList.Clear();
        moveCount++;
    }
    public void Undo()
    {
        if (Moves.Count > 0)
        {
            var move = Moves.Last();
            if (move != null)
            {
                moveCount++;
                foreach (var step in move)
                {
                    var card = step.Card;
                    var origin = step.Origin;
                    var target = step.Target;
                    var newTarget = origin;
                    if (newTarget.name != card.transform.parent.name)
                    {
                        if (newTarget.childCount > 0)
                        {
                            if (newTarget.name.Contains("Panel"))
                            {
                                var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 1);
                                if (!lastChildOfNewTarget.GetComponent<CardControl>().wasFacingUp)
                                {
                                    StartCoroutine(RotateToHideCard(lastChildOfNewTarget.transform));
                                    lastChildOfNewTarget.GetComponent<CardControl>().enabled = false;
                                }
                            }
                            else if (newTarget.CompareTag("Ground"))
                            {
                                if (newTarget.childCount > 3)
                                {
                                    var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 3);
                                    StartCoroutine(RotateToHideCard(lastChildOfNewTarget.transform));
                                    lastChildOfNewTarget.GetComponent<CardControl>().enabled = false;

                                    lastChildOfNewTarget.gameObject.SetActive(false);
                                }
                            }
                            else if (newTarget.name.Contains("Deck"))
                            {
                                // enable the last 3 cards in the ground   
                                const int cardsAppearing = 3;
                                if (groundObj.transform.childCount > cardsAppearing)
                                {
                                    int cardsLeft = groundObj.transform.childCount - Settings.drawingCardCount;
                                    int startIndex, finishIndex;
                                    if (cardsLeft < 3)
                                    {
                                        startIndex = 0;
                                        finishIndex = cardsLeft;
                                    }
                                    else
                                    {
                                        startIndex = groundObj.transform.childCount - 3 - Settings.drawingCardCount;
                                        finishIndex = groundObj.transform.childCount - 3;
                                    }

                                    for (int i = startIndex; i < finishIndex; i++)
                                    {
                                        groundObj.transform.GetChild(i).gameObject.SetActive(true);
                                    }
                                }

                                var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 1);
                                lastChildOfNewTarget.GetComponent<CardControl>().enabled = false;
                                StartCoroutine(RotateToHideCard(lastChildOfNewTarget.transform));

                                card.GetComponent<CardControl>().enabled = false;
                                StartCoroutine(RotateToHideCard(card.transform));
                            }
                        }
                        if (newTarget.name.Contains("Deck"))
                        {
                            card.GetComponent<CardControl>().enabled = false;
                            card.GetComponent<Button>().enabled = true;
                        }

                        var pos = card.GetComponent<CardControl>().earlierLocation;

                        StartCoroutine(SlideAndParent(card, newTarget, pos));
                        Moves.Remove(move);
                    }
                }
            }
        }
    }
    public void HelpButton()
    {
        Help();
    }
    public static List<List<Move>> helpList = new List<List<Move>>();
    private List<Move> Help(bool animation = true)
    {
        possibleMoves = new List<List<Move>>();
        var moves = ShowNextMove();
        while (!IsNullOrEmpty(moves))
        {
            possibleMoves.Add(moves);
            moves = ShowNextMove();
        }
        if (!IsNullOrEmpty(possibleMoves))
        {
            possibleMoves.Sort((a, b) => b.Count - a.Count);
            moves = possibleMoves.First();

        }
        while (DoesContains(helpList, moves))
        {
            possibleMoves.Remove(moves);
            if (possibleMoves.Count == 0)
            {
                moves = null;
                break;
            }
            moves = possibleMoves.First();
        }

        if (moves != null)
            helpList.Add(moves);

        if (IsNullOrEmpty(moves))
        {
            if (IsThereAMove())
                VibrateDeck();
            else
                GameOver(GameOverType.Lose);
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
                //HelpHistory.Add(move);
            }
            if (animation)
                StartCoroutine(SlideCard(helpMoves, true));
            return helpMoves;
        }
        List<Move> empty = null;
        return empty;
    }

    private bool IsItDumbMove(List<Move> result)
    {
        // return true => it is dumb move
        var origin = result.First().Origin;
        var target = result.First().Target;
        var card = result.First().Card;

        if (card.transform.GetSiblingIndex() == 0) // for the card K => if it is already in the empty panel, it doesnt need to change position
        {
            if (target.childCount == 0)
                return true;
            else
                return false;
        }
        else if (origin.childCount >= 2 && target.childCount >= 1) // if its already in the same color and value, its donest need to move
        {
            var bigBrotherOfOriginCardIndex = result.First().Card.transform.GetSiblingIndex() - 1;
            var bigBrotherOfOriginCard = origin.GetChild(bigBrotherOfOriginCardIndex);

            if (bigBrotherOfOriginCard.GetComponent<CardControl>().isFacingUp)
            {
                var bigBrotherOfOriginCardValue = int.Parse(bigBrotherOfOriginCard.name.Substring(1));

                var targetCard = target.GetChild(target.childCount - 1);
                int targetCardValue = int.Parse(targetCard.name.Substring(1));

                if (bigBrotherOfOriginCardValue == targetCardValue)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
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
    private void VibrateDeck()
    {
        StartCoroutine(Shake(deckObj));
    }
    private IEnumerator Shake(GameObject deckObj)
    {
        float s = 0.05f;
        float seconds;
        float t;
        var dpPos = deckObj.GetComponent<RectTransform>().position;
        Vector3 left = new Vector3(dpPos.x + 5, dpPos.y, dpPos.z);
        Vector3 right = new Vector3(dpPos.x - 5, dpPos.y, dpPos.z);
        for (int i = 0; i < 5; i++)
        {
            seconds = s;
            t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                deckObj.GetComponent<RectTransform>().position = Vector3.Lerp(deckObj.GetComponent<RectTransform>().position, left, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            seconds = s;
            t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                deckObj.GetComponent<RectTransform>().position = Vector3.Lerp(deckObj.GetComponent<RectTransform>().position, right, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }
        seconds = s;
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            deckObj.GetComponent<RectTransform>().position = Vector3.Lerp(deckObj.GetComponent<RectTransform>().position, dpPos, Mathf.SmoothStep(0f, 1f, t));
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
            StartCoroutine(SlideAndDisappear(movingDummies[i], positions[i], willDisappear));
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
    private IEnumerator SlideAndDisappear(GameObject movingDummy, Vector3 pos, bool willDisappear, float time = 1f)
    {
        float seconds = time;
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
        if (moves == null)
        {
            if (deckObj.transform.childCount > 0)
                DealFromDeck(false);
            else
                deckObj.GetComponent<Button>().onClick.Invoke();
        }
        else
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
                            lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;

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
                            lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;

                        }
                        originParent.GetComponent<VerticalLayoutGroup>().spacing = CardControl.CalculateSpacing(originParent); // set the spacing for the panel layout
                    }
                    else if (originParent.CompareTag("Ground")) // 6  => 1 2 (3 4 5 )
                    {
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
    public void RestartGame()
    {
        var q = Instantiate(questionPrefab);
        q.transform.SetParent(canvas.transform);
        q.transform.GetComponent<RectTransform>().localPosition = Vector2.zero;
        var yesButton = GameObject.FindGameObjectWithTag("YesButton");
        yesButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadGameScreen(); });
    }
    public void LoadMainMenu()
    {
        var q = Instantiate(questionPrefab);
        q.transform.SetParent(canvas.transform);
        q.transform.GetComponent<RectTransform>().localPosition = Vector2.zero;
        var yesButton = GameObject.FindGameObjectWithTag("YesButton");
        yesButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadMainScreen(); });
    }
}