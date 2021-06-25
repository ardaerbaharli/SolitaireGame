using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject timeText;
    [SerializeField] GameObject moveCountText;

    void Start()
    {
        StatisticController.Time = 0;
    }

    private void Update()
    {
        if (!GameControl.instance.isGameOver)
        {
            StatisticController.Time += Time.deltaTime;
            int sec = (int)(StatisticController.Time % 60);
            int min = (int)(StatisticController.Time / 60);

            string secs = sec.ToString();
            if (sec < 10)
                secs = $"0{sec}";

            string mins = sec.ToString();
            if (min < 10)
                mins = $"0{min}";

            timeText.GetComponent<Text>().text = $"{mins}:{secs}";

            moveCountText.GetComponent<Text>().text = StatisticController.MoveCount.ToString();

        }
        else
        {
            Destroy(timeText.transform.parent.gameObject);
            Destroy(moveCountText.transform.parent.gameObject);
            gameObject.GetComponent<GameUI>().enabled = false;
        }
    }
}