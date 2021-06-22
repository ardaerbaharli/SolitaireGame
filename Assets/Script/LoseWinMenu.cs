using UnityEngine;
using UnityEngine.UI;

public class LoseWinMenu : MonoBehaviour
{
    [SerializeField] GameObject playAgainButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] GameObject gameOverType;
    [SerializeField] GameObject gameControl;

    private void Start()
    {
        gameControl = GameObject.FindGameObjectWithTag("GameControl");
        mainMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadMainScreen(); });
        playAgainButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadGameScreen(); });
        var type = gameControl.GetComponent<GameControl>().gameOverType;
        if (type == GameControl.GameOverType.Lose)
            gameOverType.GetComponent<Text>().text = "YOU LOST";
        else if (type == GameControl.GameOverType.Win)
            gameOverType.GetComponent<Text>().text = "YOU WON";
    }
}