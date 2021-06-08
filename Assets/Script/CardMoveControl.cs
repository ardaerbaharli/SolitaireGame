using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMoveControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool isMoving;
    public bool isChangingPostiion;
    public bool isFacingUp;
    public GameObject playcardsPanel;
    public GameObject acesPanel;
    private void Start()
    {
        isMoving = false;
        isChangingPostiion = false;
        playcardsPanel = GameObject.FindGameObjectWithTag("PlaycardsPanel");
        acesPanel = GameObject.FindGameObjectWithTag("AcesPanel");

        // FOR TESTING 
        if (Settings.drawingCardCount == 0)
        {
            Settings.drawingCardCount = 1;
            Settings.deckRefreshCount = 999;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isChangingPostiion = true;
        isMoving = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isMoving = false;
    }

    public void OnDrag(PointerEventData data)
    {
        if (isMoving)
        {
            int siblingIndex = gameObject.transform.GetSiblingIndex();
            int childCount = gameObject.transform.parent.childCount;
            for (int i = siblingIndex; i < childCount; i++)
            {
                gameObject.transform.parent.GetChild(i).position = data.position;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (isChangingPostiion && !isMoving)
        {
            bool didItMove = false;
            bool canMove = false;
            int panelIndex = Int32.Parse(collision.name);
            var originParent = gameObject.transform.parent;

            var originCardName = gameObject.name;
            char originCardSuit = originCardName[0];
            int originCardValue = int.Parse(originCardName.Substring(1, originCardName.Length - 1));

            if (collision.CompareTag("PlaceholderPanel")) // = ground cards = target
            {
                var targetParent = playcardsPanel.transform.GetChild(panelIndex);

                if (targetParent.childCount > 0)
                {
                    var targetCard = targetParent.GetChild(targetParent.childCount - 1);
                    var targetCardName = targetCard.name;
                    char targetCardSuit = targetCardName[0];
                    int targetCardValue = int.Parse(targetCardName.Substring(1, targetCardName.Length - 1));

                    if (targetCardValue - originCardValue == 1 && IsPlaygroudCardSuitTrue(targetCardSuit, originCardSuit))
                        canMove = true;
                }
                else if (targetParent.childCount == 0) // if the playground panel is empty
                {
                    // then only K => 13 can go there
                    if (originCardValue == 13)
                        canMove = true;
                }
                if (canMove)
                {
                    if (originParent.CompareTag("Ground") || originParent.CompareTag("AcePlaceholderPanel"))
                    {
                        didItMove = true;
                        gameObject.transform.SetParent(targetParent);
                        targetParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(targetParent); ; // set the spacing for the panel layout

                        Move move = new Move();
                        move.Origin = originParent;
                        move.Card = gameObject;
                        move.Target = targetParent;
                        GameControl.AddMove(move);

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
                        didItMove = true;

                        int holdingIndex = gameObject.transform.GetSiblingIndex();
                        int lastChildIndex = gameObject.transform.parent.childCount - 1;
                        int cardCount = lastChildIndex - holdingIndex + 1;
                        for (int i = 0; i < cardCount; i++)
                        {
                            var card = originParent.GetChild(holdingIndex);
                            card.SetParent(targetParent);

                            Move move = new Move();
                            move.Origin = originParent;
                            move.Card = card.gameObject;
                            move.Target = targetParent;
                            GameControl.AddMove(move); 
                        }

                        if (originParent.childCount > 0)
                        {
                            // change the image and enable the cardmovecontrol script
                            var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                            lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                            lastChildOfTheOriginParent.GetComponent<CardMoveControl>().enabled = true;
                            lastChildOfTheOriginParent.GetComponent<CardMoveControl>().isFacingUp = true;
                        }

                        originParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(originParent);  // set the spacing for the panel layout
                        targetParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(targetParent);  // set the spacing for the panel layout
                    }
                }
            }
            else if (collision.CompareTag("AcePlaceholderPanel")) // = target
            {
                if (originParent.CompareTag("Ground") || originParent.CompareTag("PlaycardsPanelChildren"))
                {
                    var targetParent = acesPanel.transform.GetChild(panelIndex); // next parent                    

                    if (targetParent.childCount > 0)
                    {
                        var targetCard = targetParent.GetChild(targetParent.childCount - 1);
                        var targetCardName = targetCard.name;
                        char targetCardSuit = targetCardName[0];
                        int targetValue = int.Parse(targetCardName.Substring(1, targetCardName.Length - 1));

                        if (originCardValue - targetValue == 1 && targetCardSuit == originCardSuit)
                            canMove = true;
                    }
                    else if (targetParent.childCount == 0) // if the ace panel is empty
                    {
                        if (originCardValue == 1) // then only A => 1 can go there
                            canMove = true;
                    }

                    if (canMove)
                    {
                        didItMove = true;
                        gameObject.transform.SetParent(targetParent); // change the parent of the card

                        Move move = new Move();
                        move.Origin = originParent;
                        move.Card = gameObject;
                        move.Target = targetParent;
                        GameControl.AddMove(move);

                        if (originParent.CompareTag("PlaycardsPanelChildren"))
                        {
                            if (originParent.childCount > 0)
                            {
                                // change the image and enable the cardmovecontrol script
                                var lastChildOfTheOriginParent = originParent.GetChild(originParent.childCount - 1);
                                lastChildOfTheOriginParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOriginParent.name);
                                lastChildOfTheOriginParent.GetComponent<CardMoveControl>().enabled = true;
                                lastChildOfTheOriginParent.GetComponent<CardMoveControl>().isFacingUp = true;
                            }
                            originParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(originParent); // set the spacing for the panel layout
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
                }
            }

            if (didItMove)
                GameControl.moveCount++;

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