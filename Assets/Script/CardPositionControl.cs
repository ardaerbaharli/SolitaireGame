using UnityEngine;
public class CardPositionControl : MonoBehaviour
{
    [SerializeField] GameObject playcardsObj;
    private const int columnCount = 7;


    void Update()
    {
        for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var currentColumn = playcardsObj.transform.GetChild(columnIndex); // get current column in playground
            var cardCountInColumn = currentColumn.childCount;
            for (int cardIndex = 0; cardIndex < cardCountInColumn; cardIndex++)
            {
                var card = currentColumn.GetChild(cardIndex); // get card in the index of the column
                double posX = card.GetComponent<RectTransform>().anchoredPosition.x;
                if (posX < 0 && columnIndex != 0)// if it is over its left side of the border of the column panel AND it is not the first column in the left
                {
                    var nextColumn = playcardsObj.transform.GetChild(columnIndex - 1); // get the column to move the card
                    nextColumn.GetChild(nextColumn.childCount - 1).GetComponent<CardMoveControl>().enabled = false; // disable movement for the card in the next column (because it will be overlapped by another card)
                    card.SetParent(nextColumn); // change the parent of the card                    
                    currentColumn.GetChild(cardIndex - 1).GetComponent<CardMoveControl>().enabled = true; // enable the movement for the card in the before column (because it will not be overlapped anymore)
                }
                else if (posX > 142.6969f && columnIndex != columnCount - 1) // if it is over its right side of the border of the column panel AND it is not the last column in the right
                {
                    var nextColumn = playcardsObj.transform.GetChild(columnIndex + 1); // get the column to move the card
                    nextColumn.GetChild(nextColumn.childCount - 1).GetComponent<CardMoveControl>().enabled = false; // disable movement for the card in the next column(because it will be overlapped by another card)
                    card.SetParent(nextColumn); // change the parent of the card
                    if (currentColumn.childCount > 0)
                        currentColumn.GetChild(cardIndex - 1).GetComponent<CardMoveControl>().enabled = true; // enable the movement for the card in the before column (because it will not be overlapped anymore)
                }
            }
        }
    }
}
