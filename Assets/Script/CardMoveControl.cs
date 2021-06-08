using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardMoveControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool isMoving;
    public bool isChangingPostiion;
    public GameObject playcardsPanel;
    public GameObject acesPanel;
    private void Start()
    {
        isMoving = false;
        isChangingPostiion = false;
        playcardsPanel = GameObject.FindGameObjectWithTag("PlaycardsPanel");
        acesPanel = GameObject.FindGameObjectWithTag("AcesPanel");

        // FOR TESTING 
        Settings.drawingCardCount = 1;
        Settings.deckRefreshCount = 999;
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
            var originParent = gameObject.transform.parent;
            if (collision.CompareTag("PlaceholderPanel")) // target
            {
                int panelIndex = Int32.Parse(collision.name);
                var updatedParent = playcardsPanel.transform.GetChild(panelIndex); // next parent       

                bool canMove = false;

                var cardName = gameObject.name;
                char cardSuit = cardName[0];
                int cardValue = int.Parse(cardName.Substring(1, cardName.Length - 1));
                if (updatedParent.childCount > 0)
                {
                    var targetCard = updatedParent.GetChild(updatedParent.childCount - 1);
                    var targetCardName = targetCard.name;
                    char targetCardSuit = targetCardName[0];
                    int targetValue = int.Parse(targetCardName.Substring(1, targetCardName.Length - 1));

                    if (targetValue - cardValue == 1 && IsPlaygroudCardSuitTrue(targetCardSuit, cardSuit))
                        canMove = true;
                }
                else if (updatedParent.childCount == 0)
                {
                    // if the panel is empty, then only K => 13 can go there
                    if (cardValue == 13)
                        canMove = true;
                }

                if (originParent.CompareTag("Ground") || originParent.CompareTag("AcePlaceholderPanel"))
                {
                    if (canMove)
                    {
                        didItMove = true;
                        gameObject.transform.SetParent(updatedParent);
                        updatedParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(updatedParent); ; // set the spacing for the panel layout
                    }

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
                    if (canMove)
                    {
                        didItMove = true;

                        int holdingIndex = gameObject.transform.GetSiblingIndex();
                        int lastChildIndex = gameObject.transform.parent.childCount - 1;
                        int cardCount = lastChildIndex - holdingIndex + 1;
                        var parent = gameObject.transform.parent;
                        for (int i = 0; i < cardCount; i++)
                        {
                            parent.GetChild(holdingIndex).SetParent(updatedParent);
                        }

                        if (originParent.childCount > 0)
                        {
                            // change the image and enable the cardmovecontrol script
                            var lastChildOfTheOlderParent = originParent.GetChild(originParent.childCount - 1);
                            lastChildOfTheOlderParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOlderParent.name);
                            lastChildOfTheOlderParent.GetComponent<CardMoveControl>().enabled = true;
                        }

                        originParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(originParent);  // set the spacing for the panel layout
                                                                                                                    //     updatedParent.GetComponent<VerticalLayoutGroup>().spacing = CalculateSpacing(updatedParent); ; // set the spacing for the panel layout
                    }
                }
            }
            else if (collision.CompareTag("AcePlaceholderPanel")) // target
            {
                if (originParent.CompareTag("Ground") || originParent.CompareTag("PlaycardsPanelChildren"))
                {
                    int panelIndex = Int32.Parse(collision.name);
                    var updatedParent = acesPanel.transform.GetChild(panelIndex); // next parent       

                    bool canMove = false;

                    var cardName = gameObject.name;
                    char cardSuit = cardName[0];

                    int cardValue = int.Parse(cardName.Substring(1, cardName.Length - 1));
                    if (updatedParent.childCount > 0)
                    {
                        var targetCard = updatedParent.GetChild(updatedParent.childCount - 1);
                        var targetCardName = targetCard.name;
                        char targetCardSuit = targetCardName[0];

                        int targetValue = int.Parse(targetCardName.Substring(1, targetCardName.Length - 1));
                        if (cardValue - targetValue == 1 && targetCardSuit == cardSuit)
                            canMove = true;
                    }
                    else if (updatedParent.childCount == 0)
                    {
                        // if the ace panel is empty, then only A => 1 can go there
                        if (cardValue == 1)
                            canMove = true;
                    }

                    if (canMove)
                    {
                        didItMove = true;
                        gameObject.transform.SetParent(updatedParent); // change the parent of the card

                        if (originParent.CompareTag("PlaycardsPanelChildren"))
                        {
                            if (originParent.childCount > 0)
                            {
                                // change the image and enable the cardmovecontrol script
                                var lastChildOfTheOlderParent = originParent.GetChild(originParent.childCount - 1);
                                lastChildOfTheOlderParent.GetComponent<Image>().sprite = Resources.Load<Sprite>(lastChildOfTheOlderParent.name);
                                lastChildOfTheOlderParent.GetComponent<CardMoveControl>().enabled = true;
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
            {
                GameControl.moveCount++;
                GameControl.score += 1000 / GameControl.moveCount * 2;
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

    private float CalculateSpacing(Transform transform)
    {
        float spacing = -1030f + (transform.childCount * 90f);
        if (spacing > -200f)
            spacing = -200f;
        else if (spacing < -940f)
            spacing = -940f;
        return spacing;
    }
}