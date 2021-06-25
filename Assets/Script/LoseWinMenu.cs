using UnityEngine;
using UnityEngine.UI;

public class LoseWinMenu : MonoBehaviour
{
    [SerializeField] GameObject playAgainButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] GameObject gameOverType;
    [SerializeField] GameObject gameControl;
    [SerializeField] GameObject bestMoveText;
    [SerializeField] GameObject bestTimeText;
    [SerializeField] GameObject timeText;
    [SerializeField] GameObject moveText;

    private void Start()
    {
        //if (GameObject.FindGameObjecstWithTag("LoseWinMenu") != null)
        //    Destroy(gameObject);

        gameControl = GameObject.FindGameObjectWithTag("GameControl");

        mainMenuButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadMainScreen(); });
        playAgainButton.GetComponent<Button>().onClick.AddListener(delegate { SceneLoader.instance.LoadGameScreen(); });

        var type = gameControl.GetComponent<GameControl>().gameOverType;
        if (type == GameControl.GameOverType.Lose)
            gameOverType.GetComponent<Text>().text = "YOU LOST";
        else if (type == GameControl.GameOverType.Win)
            gameOverType.GetComponent<Text>().text = "YOU WON";

        var _bestTime = StatisticController.BestTime;
        var _time = StatisticController.Time;

        int bestTimeMin = Mathf.FloorToInt(_bestTime / 60);
        _bestTime -= bestTimeMin * 60;
        int bestTimeSec = Mathf.FloorToInt(_bestTime % 60);

        int timeMin = Mathf.FloorToInt(_time / 60);
        _time -= bestTimeMin * 60;
        int timeSec = Mathf.FloorToInt(_time % 60);

        string bMin = bestTimeMin.ToString();
        if (bestTimeMin < 10)
            bMin = $"0{bestTimeMin}";

        string bSec = bestTimeSec.ToString();
        if (bestTimeSec < 10)
            bSec = $"0{bestTimeSec}";

        bestTimeText.GetComponent<Text>().text = $"{bMin}:{bSec}";

        string tMin = timeMin.ToString();
        if (timeMin < 10)
            tMin = $"0{timeMin}";

        string tSec = timeSec.ToString();
        if (timeSec < 10)
            tSec = $"0{timeSec}";

        timeText.GetComponent<Text>().text = $"{tMin}:{tSec}";

        moveText.GetComponent<Text>().text = StatisticController.MoveCount.ToString();
        bestMoveText.GetComponent<Text>().text = StatisticController.BestMoveCount.ToString();
    }
}