﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseManager
{
    private static bool _isPaused = false;
    private static bool _systemPaused = false;
    private static List<MonoBehaviour> _systemPausers = new List<MonoBehaviour>();
    public static PauseMenu pauseMenu;

    public static bool Paused
    {
        get { return _isPaused || _systemPaused; }
    }

    /*
    public static bool SetPaused
    {
        set { _isPaused = value; }
    }
    */

    public static void TogglePause()
    {
        if (_isPaused)
            UnPause();
        else
            Pause();
    }

    public static void Pause()
    {
        _isPaused = true;

        if (pauseMenu != null)
            pauseMenu.gameObject.SetActive(true);

        if (_systemPaused == false)
        {
            PauseEffects();
        }
    }

    public static void UnPause()
    {
        _isPaused = false;

        if (pauseMenu != null)
            pauseMenu.CloseMenu();

        if (_systemPaused == false)
        {
            UnPauseEffects();
        }
    }

    public static void ShowCursor()
    {
        if (_systemPaused == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public static void SystemPause(MonoBehaviour pausedBy)
    {
        _systemPausers.Add(pausedBy);
        _systemPaused = true;

        PauseEffects();
    }

    public static void SystemUnPause(MonoBehaviour pausedBy)
    {
        _systemPausers.Remove(pausedBy);
        if (_systemPausers.Count == 0)
        {
            _systemPaused = false;

            UnPause();
        }
    }

    private static void PauseEffects()
    {
        //AudioManager.PauseAll();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    private static void UnPauseEffects()
    {
        //AudioManager.ResumeAll();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }
}
