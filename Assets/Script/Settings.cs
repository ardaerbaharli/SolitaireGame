using UnityEngine;

public class Settings : MonoBehaviour
{
    public static int drawingCardCount { get; set; }
    public static int deckRefreshCount { get; set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        drawingCardCount = 1;
        deckRefreshCount = 999;
    }
    public void StartGame()
    {
        SceneLoader.instance.LoadGameScreen();
    }
    public void OneCardToggle()
    {
        drawingCardCount = 1;
    }
    public void ThreeCardToggle()
    {
        drawingCardCount = 3;
    }
    public void NormalToggle()
    {
        deckRefreshCount = 999;
    }
    public void VegasToggle()
    {
        deckRefreshCount = drawingCardCount - 1;
    }
}
