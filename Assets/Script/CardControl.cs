using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardControl : MonoBehaviour, IDragHandler, IPointerDownHandler//, IPointerUpHandler
{
    public bool isMoving;
    public bool isChangingPostiion;
    public bool isFacingUp;
    public bool wasFacingUp;
    public bool isDeckCard;
    public bool isPlayable;
    //public bool isK;
    //public bool didGoToEmptySpot;
    public bool isDummy;

    public GameObject playcardsPanel;
    public GameObject acesPanel;
    public GameObject groundObj;
    //public GameObject canvas;

    //public Transform test;
    //private Transform origin;

    void Start()
    {
        isMoving = false;
        isChangingPostiion = false;
        playcardsPanel = GameObject.FindGameObjectWithTag("PlaycardsPanel");
        acesPanel = GameObject.FindGameObjectWithTag("AcesPanel");
        groundObj = GameObject.FindGameObjectWithTag("Ground");
        //canvas = GameObject.FindGameObjectWithTag("Canvas");
        //test = GameObject.Find("Panel (8)").transform;
    }
    void Update()
    {
        if (!wasFacingUp && isFacingUp)
            if (gameObject.transform.parent.childCount > gameObject.transform.GetSiblingIndex() + 1)
            {
                wasFacingUp = true;
            }

        if (Input.GetMouseButtonUp(0))
        {
            isMoving = false;
            gameObject.GetComponent<Canvas>().overrideSorting = false;
            gameObject.GetComponent<Canvas>().sortingOrder = 1;

            if (gameObject.transform.childCount > 0)
            {
                var abc = gameObject.transform.GetChild(0);
                while (abc != null)
                {
                    abc.SetParent(gameObject.transform.parent);
                    if (abc.transform.childCount > 0)
                        abc = abc.transform.GetChild(0);
                    else break;
                }
            }

            StartCoroutine(RefreshLayout());
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        isChangingPostiion = true;
        isMoving = true;

        int siblingIndex = gameObject.transform.GetSiblingIndex();
        int childCount = gameObject.transform.parent.childCount;

        gameObject.GetComponent<Canvas>().overrideSorting = true;
        gameObject.GetComponent<Canvas>().sortingOrder = 2;
        if (childCount - siblingIndex != 1)
        {
            for (int i = childCount - 1; i > siblingIndex; i--)
            {
                gameObject.transform.parent.GetChild(i).SetParent(gameObject.transform.parent.GetChild(i - 1).transform);
            }
        }

        //int siblingIndex = gameObject.transform.GetSiblingIndex();
        //int childCount = gameObject.transform.parent.childCount;

        //for (int i = siblingIndex; i < childCount; i++)
        //{
        //    gameObject.transform.parent.GetChild(siblingIndex).transform.SetParent(test);
        //}
    }

    private IEnumerator RefreshLayout()
    {
        yield return new WaitForSeconds(0.1f);
        //if (gameObject.transform.parent.name.Contains("8"))
        //{
        //    int siblingIndex = gameObject.transform.GetSiblingIndex();
        //    int childCount = gameObject.transform.parent.childCount;

        //    for (int i = siblingIndex; i < childCount; i++)
        //    {
        //        gameObject.transform.parent.GetChild(i).transform.SetParent(origin);
        //    }
        //}
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.GetComponent<RectTransform>()); // refresh layout
    }

    public void OnDrag(PointerEventData data)
    {
        if (isMoving)
        {
            int siblingIndex = gameObject.transform.GetSiblingIndex();
            int childCount = gameObject.transform.parent.childCount;

            for (int i = siblingIndex; i < childCount; i++)
            {
                gameObject.transform.parent.GetChild(i).transform.position = data.position;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D target)
    {
        if (!isDummy && !isMoving && isChangingPostiion)
        {
            bool canMove = false;
            int panelIndex = int.Parse(target.name);
            var originParent = gameObject.transform.parent;

            var originCardName = gameObject.name;
            char originCardSuit = originCardName[0];
            int originCardValue = int.Parse(originCardName.Substring(1));

            if (target.CompareTag("PlaceholderPanel")) // = playground cards = target
            {
                var targetParent = playcardsPanel.transform.GetChild(panelIndex);

                if (targetParent.childCount > 0)
                {
                    var targetCard = targetParent.GetChild(targetParent.childCount - 1);
                    if (!targetCard.GetComponent<CardControl>().isDummy)
                    {
                        var targetCardName = targetCard.name;
                        char targetCardSuit = targetCardName[0];
                        int targetCardValue = int.Parse(targetCardName.Substring(1));

                        if (targetCardValue - originCardValue == 1 && IsPlaygroudCardSuitTrue(targetCardSuit, originCardSuit))
                            canMove = true;
                    }
                }
                else if (targetParent.childCount == 0) // if the playground panel is empty
                {
                    // then only K => 13 can go there
                    if (originCardValue == 13)
                    {
                        canMove = true;
                        //gameObject.GetComponent<CardControl>().didGoToEmptySpot = true;
                    }
                }

                if (canMove)
                {
                    if (originParent.CompareTag("Ground") || originParent.CompareTag("AcePlaceholderPanel"))
                    {
                        gameObject.transform.SetParent(targetParent);

                        targetParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(targetParent); ; // set the spacing for the panel layout                       

                        Move move = new Move();
                        move.Origin = originParent;
                        move.Card = gameObject;
                        move.Target = targetParent;
                        GameControl.AddMove(move.ToList());

                        if (originParent.CompareTag("Ground"))
                        {
                            gameObject.transform.GetComponent<CardControl>().isDeckCard = false;

                            int groundCardCount = originParent.childCount;
                            // var groundObj = originParent;
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
                        int holdingIndex = gameObject.transform.GetSiblingIndex();
                        int lastChildIndex = gameObject.transform.parent.childCount - 1;
                        int holdingCardsCount = lastChildIndex - holdingIndex + 1;

                        var moves = new List<Move>();

                        for (int i = 0; i < holdingCardsCount; i++)
                        {
                            var card = originParent.GetChild(holdingIndex);
                            card.SetParent(targetParent);

                            Move move = new Move();
                            move.Origin = originParent;
                            move.Card = card.gameObject;
                            move.Target = targetParent;
                            moves.Add(move);
                        }
                        GameControl.AddMove(moves);

                        if (originParent.childCount > 0)
                        {
                            // change the image and enable the cardmovecontrol script
                            var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                            lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                            lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;
                            lastChildOfTheOriginParent.GetComponent<CardControl>().enabled = true;
                            lastChildOfTheOriginParent.GetComponent<CardControl>().isFacingUp = true;
                        }

                        originParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(originParent);  // set the spacing for the panel layout
                        targetParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(targetParent);  // set the spacing for the panel layout
                    }
                }
            }
            else if (target.CompareTag("AcePlaceholderPanel")) // = target
            {
                if (originParent.CompareTag("Ground") || originParent.CompareTag("PlaycardsPanelChildren"))
                {
                    var targetParent = acesPanel.transform.GetChild(panelIndex); // next parent                    

                    if (targetParent.childCount > 0)
                    {
                        var targetCard = targetParent.GetChild(targetParent.childCount - 1);
                        if (!targetCard.GetComponent<CardControl>().isDummy)
                        {
                            var targetCardName = targetCard.name;
                            char targetCardSuit = targetCardName[0];
                            int targetValue = int.Parse(targetCardName.Substring(1));

                            if (originCardValue - targetValue == 1 && targetCardSuit == originCardSuit)
                                canMove = true;
                        }
                    }
                    else if (targetParent.childCount == 0) // if the ace panel is empty
                    {
                        if (originCardValue == 1) // then only A => 1 can go there
                            canMove = true;
                    }

                    if (canMove)
                    {
                        gameObject.transform.SetParent(targetParent); // change the parent of the card

                        Move move = new Move();
                        move.Origin = originParent;
                        move.Card = gameObject;
                        move.Target = targetParent;
                        GameControl.AddMove(move.ToList());

                        if (originParent.CompareTag("PlaycardsPanelChildren"))
                        {
                            if (originParent.childCount > 0)
                            {
                                // change the image and enable the cardmovecontrol script
                                var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                                lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                                lastChildOfTheOriginParent.GetComponent<BoxCollider2D>().enabled = true;
                                lastChildOfTheOriginParent.GetComponent<CardControl>().enabled = true;
                                lastChildOfTheOriginParent.GetComponent<CardControl>().isFacingUp = true;
                            }
                            originParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(originParent); // set the spacing for the panel layout
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
                }
            }


            isChangingPostiion = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.transform.parent.GetComponent<RectTransform>()); // refresh layout
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

    public static float CalculateSpacing(Transform transform)
    {
        float spacing = -1030f + (transform.childCount * 90f);
        if (spacing > -200f)
            spacing = -200f;
        else if (spacing < -940f)
            spacing = -940f;
        return spacing;
    }
}