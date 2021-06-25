using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticController : MonoBehaviour
{
    public static float BestTime { get; set; }
    public static int BestMoveCount { get; set; }
    public static float Time { get; set; }
    public static int MoveCount { get; set; }
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        BestTime = PlayerPrefs.GetFloat("BestTime");
        BestMoveCount = PlayerPrefs.GetInt("BestMove");
    }


    public static void UpdateBestTime(float time)
    {
        if (BestTime == 0)
        {
            BestTime = time;
            PlayerPrefs.SetFloat("BestTime", BestTime);
        }
        else
        {
            if (time < BestTime)
            {
                BestTime = time;
                PlayerPrefs.SetFloat("BestTime", BestTime);
            }
        }
    }

    public static void UpdateBestMove(int moveCount)
    {
        if (BestMoveCount == 0)
        {
            BestMoveCount = moveCount;
            PlayerPrefs.SetInt("BestMove", BestMoveCount);
        }
        else
        {
            if (moveCount < BestMoveCount)
            {
                BestMoveCount = moveCount;
                PlayerPrefs.SetInt("BestMove", BestMoveCount);
            }
        }
    }
}
