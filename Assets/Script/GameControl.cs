using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    public GameObject cardPrefab;

    [SerializeField] public GameObject groundObj;
    [SerializeField] public GameObject canvas;
    [SerializeField] public GameObject playcardsObj;
    [SerializeField] public GameObject deckObj;
    [SerializeField] public GameObject acePanel;
    [SerializeField] public GameObject undoButton;
    [SerializeField] public GameObject helpButton;
    [SerializeField] public GameObject questionPrefab;
    [SerializeField] public GameObject loseWinMenuPrefab;
    [Range(0, 10f)] private float timeScale = 1f;

    public enum GameOverType
    {
        Win,
        Lose
    };

    private List<Card> UnshuffledDeck = new List<Card>();
    private static List<List<Move>> Moves = new List<List<Move>>();
    public List<List<Move>> possibleMoves = new List<List<Move>>();


    private int remainingRefreshes;
    public static GameControl instance;
    public bool isGameOver;
    public bool didPlayerWin;
    public bool isCelebrated;
    public bool isGameOverThingsComplete;
    public GameOverType gameOverType;
    public bool fillPlayground;
    public bool isSomethingMoving;

    private void Awake()
    {
        instance = this;
        // FOR TESTING 
        if (Settings.drawingCardCount == 0)
        {
            Settings.drawingCardCount = 1;
            Settings.deckRefreshCount = 999;
        }
        //

        fillPlayground = false;
        Moves.Clear();
        StatisticController.MoveCount = 0;
        isGameOver = false;
        remainingRefreshes = Settings.deckRefreshCount;

        UnshuffledDeck = CreateADeck();
        DealPlayCards();
        DealDeck();

        for (int i = 0; i < playcardsObj.transform.childCount; i++)
        {
            playcardsObj.transform.GetChild(i).GetComponent<VerticalLayoutGroup>().spacing =
                CalculateSpacing(playcardsObj.transform.GetChild(i));
        }
    }

    private void Update()
    {
        if (instance == null)
            instance = this;

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
                            if (!panel.GetChild(j).GetComponent<CardController>().isFacingUp)
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
                            undoButton.SetActive(false);
                            helpButton.SetActive(false);
                            didPlayerWin = true;
                            isGameOver = true;
                        }
                        else
                        {
                            undoButton.SetActive(false);
                            helpButton.SetActive(false);
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

    private void GameOver(GameOverType type)
    {
        if (type == GameOverType.Win)
        {
            StatisticController.UpdateBestTime(StatisticController.Time);
            StatisticController.UpdateBestMove(StatisticController.MoveCount);
        }

        gameOverType = type;
        var lwm = Instantiate(loseWinMenuPrefab, canvas.transform);
        lwm.transform.GetComponent<RectTransform>().localPosition = Vector2.zero;

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

    private IEnumerator CelebrateWin()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < acePanel.transform.childCount; i++)
        {
            var acePanelI = acePanel.transform.GetChild(i);

            for (int j = 0; j < acePanelI.childCount; j++)
            {
                var card = acePanelI.GetChild(j);
                card.GetComponent<CardController>().enabled = true;
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


            int cardSuitIndex = (int) (card.ID / 13f);
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

                var cardObj =
                    Instantiate(cardPrefab, playcardsObj.transform.GetChild(columnIndex).transform) as GameObject;
                //cardObj.transform.SetParent(playcardsObj.transform.GetChild(columnIndex).transform);
                cardObj.name = card.ImageName;
                if (column == piece + 1)
                {
                    cardObj.GetComponent<CardController>().enabled = true;
                    cardObj.GetComponent<CardController>().isFacingUp = true;
                    cardObj.GetComponent<CardController>().earlierLocation = gameObject.transform.position;
                    cardObj.GetComponent<CardController>().lastKnownLocation = gameObject.transform.position;
                    cardObj.GetComponent<BoxCollider2D>().enabled = true;
                    cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(card.ImageName);
                }
                else
                {
                    cardObj.GetComponent<CardController>().earlierLocation = gameObject.transform.position;
                    cardObj.GetComponent<CardController>().lastKnownLocation = gameObject.transform.position;
                    cardObj.GetComponent<CardController>().enabled = false;
                    cardObj.GetComponent<CardController>().isFacingUp = false;
                    cardObj.GetComponent<Image>().sprite =
                        Resources.Load<Sprite>(GameConfig.BACK_OF_A_CARD_SPRITE_NAME);
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
            var cardObj = Instantiate(cardPrefab, deckObj.transform) as GameObject;
            cardObj.transform.SetParent(deckObj.transform);
            cardObj.transform.position = deckObj.transform.position;
            cardObj.name = card.ImageName;
            cardObj.GetComponent<CardController>().isFacingUp = false;
            cardObj.GetComponent<CardController>().enabled = false;
            var v = new Vector3(0, 180f, 0);
            var q = Quaternion.Euler(v);
            cardObj.GetComponent<RectTransform>().rotation = q;
            cardObj.GetComponent<GraphicRaycaster>().ignoreReversedGraphics = false;
            cardObj.GetComponent<CardController>().earlierLocation = gameObject.transform.position;
            cardObj.GetComponent<CardController>().lastKnownLocation = gameObject.transform.position;

            cardObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(GameConfig.BACK_OF_A_CARD_SPRITE_NAME);
            cardObj.AddComponent<Button>();
            cardObj.GetComponent<Button>().onClick.AddListener(delegate { DealFromDeckButton(); });
            cardObj.GetComponent<Button>().enabled = false;
        }

        if (deckObj.transform.childCount > 0)
            deckObj.transform.GetChild(deckObj.transform.childCount - 1).GetComponent<Button>().enabled = true;
    }

    public void DealFromDeckButton()
    {
        StartCoroutine(DealFromDeck());
    }

    public IEnumerator DealFromDeck(bool animation = true)
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
            var dummy = Instantiate(cardPrefab, parent);
            dummy.GetComponent<CardController>().isDummy = true;
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
            card.GetComponent<CardController>().enabled = true;
            card.GetComponent<BoxCollider2D>().enabled = true;
            card.GetComponent<Button>().enabled = false;

            card.GetComponent<Canvas>().overrideSorting = true;
            card.GetComponent<Canvas>().sortingOrder = (dealingCardCount + posIndex) + 3;

            Move move = new Move();
            move.Origin = deckObj.transform;
            move.Card = card;
            move.Target = groundObj.transform;
            moves.Add(move);
            card.GetComponent<CardController>().earlierLocation = deckObj.transform.position;
            card.GetComponent<CardController>().lastKnownLocation = cardPositions[posIndex];

            StartCoroutine(
                MyAnimationController.SlideAndParentToGround(card, 0.5f, cardPositions[posIndex], animation));
            posIndex++;
            StartCoroutine(MyAnimationController.RotateToRevealCard(card.transform, 0.2f));
            card.GetComponent<CardController>().isFacingUp = true;
        }

        do
        {
            yield return null;
        } while (deckObj.transform.childCount != deckCardCount - posIndex);


        for (int i = groundObj.transform.childCount - dealingCardCount; i < groundObj.transform.childCount; i++)
        {
            var card = groundObj.transform.GetChild(i).gameObject;

            card.GetComponent<Canvas>().overrideSorting = false;
        }

        if (deckObj.transform.childCount > 0)
            deckObj.transform.GetChild(deckObj.transform.childCount - 1).GetComponent<Button>().enabled = true;

        AddMove(moves);
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

                if (!Foreseer.IsThereAMove())
                {
                    GameOver(GameOverType.Lose);
                }
                else
                {
                    for (int i = groundCardCount - 1; i >= 0; i--)
                    {
                        var topCard = groundObj.transform.GetChild(i);
                        topCard.gameObject.SetActive(true);

                        topCard.GetComponent<CardController>().enabled = false;
                        topCard.GetComponent<Button>().enabled = true;

                        var m = new Move();
                        m.Card = topCard.gameObject;
                        m.Origin = topCard.parent;
                        m.Target = deckObj.transform;
                        moves.Add(m);

                        StartCoroutine(MyAnimationController.SlideToDeck(topCard.gameObject, 0.3f));
                        StartCoroutine(MyAnimationController.RotateToHideCard(topCard, 0.2f));
                    }
                }
            }
            else if (remainingRefreshes == 0)
            {
                deckObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(GameConfig.EMPTY_DECK_SPRTIE_NAME);
                GameOver(GameOverType.Lose);
            }
        }
    }

    public static void AddMove(List<Move> move)
    {
        Moves.Add(move);
        helpList.Clear();
        StatisticController.MoveCount++;
    }

    public void UndoButton()
    {
        StartCoroutine(Undo());
    }

    public IEnumerator Undo()
    {
        if (Moves.Count > 0)
        {
            var move = Moves.Last();
            if (move != null)
            {
                StatisticController.MoveCount++;

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
                                if (!lastChildOfNewTarget.GetComponent<CardController>().wasFacingUp)
                                {
                                    StartCoroutine(
                                        MyAnimationController.RotateToHideCard(lastChildOfNewTarget.transform));
                                    lastChildOfNewTarget.GetComponent<CardController>().enabled = false;
                                }
                            }
                            else if (newTarget.CompareTag("Ground"))
                            {
                                if (newTarget.childCount > 3)
                                {
                                    var lastChildOfNewTarget = newTarget.GetChild(newTarget.childCount - 3);
                                    StartCoroutine(
                                        MyAnimationController.RotateToHideCard(lastChildOfNewTarget.transform));
                                    lastChildOfNewTarget.GetComponent<CardController>().enabled = false;

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
                                lastChildOfNewTarget.GetComponent<CardController>().enabled = false;
                                StartCoroutine(MyAnimationController.RotateToHideCard(lastChildOfNewTarget.transform));
                            }
                        }

                        if (newTarget.name.Contains("Deck"))
                        {
                            //StartCoroutine(RotateToHideCard(card.transform));

                            card.GetComponent<CardController>().enabled = false;
                            card.GetComponent<BoxCollider2D>().enabled = false;
                            card.GetComponent<Button>().enabled = true;
                            StartCoroutine(MyAnimationController.SlideAndParent(card, newTarget, 0.3f));
                        }
                        else
                        {
                            StartCoroutine(MyAnimationController.SlideAndParent(move.First().Card, move.First().Origin,
                                0.3f));
                        }

                        while (target.name == card.transform.parent.name)
                            yield return null;
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
        possibleMoves = Foreseer.GetAllPossibleMoves();
        List<Move> moves = new List<Move>();

        if (!Helpers.IsNullOrEmpty(possibleMoves))
        {
            possibleMoves.Sort((a, b) => b.Count - a.Count);
            moves = possibleMoves.First();
        }

        while (Helpers.DoesContains(helpList, moves))
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

        if (Helpers.IsNullOrEmpty(moves))
        {
            if (Foreseer.IsThereAMove())
            {
                if (animation)
                    StartCoroutine(MyAnimationController.Shake(deckObj));
            }
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
                StartCoroutine(MyAnimationController.SlideHelpCard(helpMoves, true));
            return helpMoves;
        }

        List<Move> empty = null;
        return empty;
    }


    private IEnumerator AutoMove(List<Move> moves)
    {
        if (moves == null)
        {
            if (deckObj.transform.childCount > 0)
                StartCoroutine(DealFromDeck(false));
            else
                deckObj.GetComponent<Button>().onClick.Invoke();
        }
        else
        {
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

                        targetParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(targetParent);
                        ; // set the spacing for the panel layout                 

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
                            lastChildOfTheOriginParent.GetComponent<Image>().sprite =
                                Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                            lastChildOfTheOriginParent.GetComponent<CardController>().enabled = true;
                            lastChildOfTheOriginParent.GetComponent<CardController>().isFacingUp = true;
                            lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;
                        }

                        originParent.GetComponent<VerticalLayoutGroup>().spacing =
                            CalculateSpacing(originParent); // set the spacing for the panel layout
                        targetParent.GetComponent<VerticalLayoutGroup>().spacing =
                            CalculateSpacing(targetParent); // set the spacing for the panel layout
                    }
                }
                else if (move.Target.parent.name.Contains("AcesPs"))
                {
                    card.transform.SetParent(targetParent); // change the parent of the card              

                    if (originParent.CompareTag("PlaycardsPanelChildren"))
                    {
                        if (originParent.childCount > 0)
                        {
                            // change the image and enable the cardmovecontrol script
                            var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                            lastChildOfTheOriginParent.GetComponent<Image>().sprite =
                                Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                            lastChildOfTheOriginParent.GetComponent<CardController>().enabled = true;
                            lastChildOfTheOriginParent.GetComponent<CardController>().isFacingUp = true;
                            lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;
                        }

                        originParent.GetComponent<VerticalLayoutGroup>().spacing =
                            CalculateSpacing(originParent); // set the spacing for the panel layout
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

                LayoutRebuilder.ForceRebuildLayoutImmediate(card.transform.parent
                    .GetComponent<RectTransform>()); // refresh layout
            }

            yield return null;
        }
    }

    public void LoadNewGame()
    {
        var q = Instantiate(questionPrefab, canvas.transform);
        q.transform.GetComponent<RectTransform>().localPosition = Vector2.zero;
        var yesButton = GameObject.FindGameObjectWithTag("YesButton");
        yesButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadGameScreen(); });
    }

    public void LoadMainMenu()
    {
        var q = Instantiate(questionPrefab, canvas.transform);
        q.transform.SetParent(canvas.transform);
        q.transform.GetComponent<RectTransform>().localPosition = Vector2.zero;
        var yesButton = GameObject.FindGameObjectWithTag("YesButton");
        yesButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadMainScreen(); });
    }

    public static float CalculateSpacing(Transform transform, int extraChildCount = 0)
    {
        float spacing = -1480f + ((transform.childCount + extraChildCount) * 50f);
        if (spacing > -180f)
            spacing = -180f;
        else if (spacing < -1430f)
            spacing = -1430f;
        return spacing;
    }
}