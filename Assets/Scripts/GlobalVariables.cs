using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    [Range(600, 1000f)] public static float ExtensionLimit = 750f;
    [Range(0f, 10f)] public static float HighVariation = 1f;
    public static int Seed = Random.Range(0, 10000);
    public static bool lowRes = true;
    public static int position;
    public static float timeLap;

    public static string FormatTime(float value)
    {
        float timeInSec = Mathf.Floor(value);
        return Mathf.Floor(timeInSec/60).ToString("00") + ":" + (timeInSec%60).ToString("00") + ":" + Mathf.Floor((value - timeInSec)*10000).ToString("0000");
    }
}
