using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static int drawingCardCount { get; set; }
    public static int deckRefreshCount { get; set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //drawingCardCount = 1;
        //deckRefreshCount = 999;
        GetPrefs();
    }

    private void GetPrefs()
    {
        drawingCardCount = PlayerPrefs.GetInt("CardCount", 1);

        if (drawingCardCount == 1)
            GameObject.Find("1Card").GetComponent<Toggle>().isOn = true;
        else
            GameObject.Find("3Cards").GetComponent<Toggle>().isOn = true;


        deckRefreshCount = PlayerPrefs.GetInt("GameType", 999);
        if (deckRefreshCount == 999) // => normal
        {
            GameObject.Find("Normal").GetComponent<Toggle>().isOn = true;
        }
        else // => vegas
        {
            GameObject.Find("Vegas").GetComponent<Toggle>().isOn = true;
            deckRefreshCount = drawingCardCount - 1;
        }
    }

    public void StartGame()
    {
        SceneLoader.instance.LoadGameScreen();
    }
    public void OneCardToggle()
    {
        drawingCardCount = 1;
        PlayerPrefs.SetInt("CardCount", drawingCardCount);
    }
    public void ThreeCardToggle()
    {
        drawingCardCount = 3;
        PlayerPrefs.SetInt("CardCount", drawingCardCount);
    }
    public void NormalToggle()
    {
        deckRefreshCount = 999;
        PlayerPrefs.SetInt("GameType", deckRefreshCount);
    }
    public void VegasToggle()
    {
        deckRefreshCount = drawingCardCount - 1;
        PlayerPrefs.SetInt("GameType", deckRefreshCount);
    }
}
