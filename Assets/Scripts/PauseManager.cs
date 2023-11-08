using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private static bool paused = false;

    public static bool Paused
    {
        get { return paused; }

    }
}
