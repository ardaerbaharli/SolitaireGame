using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsx : MonoBehaviour
{
    public static void SetBool(string name, bool booleanValue)
    {
        PlayerPrefs.SetInt(name, booleanValue ? 1 : 0);
    }

    public static bool GetBool(string name)
    {
        return PlayerPrefs.GetInt(name) == 1 ? true : false;
    }

    public static bool GetBool(string name, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(name))
        {
            return GetBool(name);
        }
        SetBool(name, defaultValue);
        return defaultValue;
    }
}