using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject timeText;
    [SerializeField] GameObject moveCountText;
    private int initialMoveCount;
    private float time;
    void Start()
    {
        time = 0;
        initialMoveCount = GameControl.moveCount;
    }

    private void Update()
    {
        time += Time.deltaTime;
        int sec = (int)(time % 60);
        int mins = (int)(time / 60);
        string secs = sec.ToString();
        if (sec < 10) secs = $"0{sec}";
        timeText.GetComponent<Text>().text = $"{mins}:{secs}";


        if (GameControl.moveCount != initialMoveCount)
        {
            initialMoveCount = GameControl.moveCount;
            moveCountText.GetComponent<Text>().text = GameControl.moveCount.ToString();
        }
    }
}