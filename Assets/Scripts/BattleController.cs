using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public static BattleController singleton;

    private static bool[] _readyPlayers;
    public static int numReadyPlayers = 0;

    public List<UpgradeController> upgradeControllers = new List<UpgradeController>();
    public EnemySpawner enemySpawner;

    void Start()
    {
        if (singleton != null)
            Debug.LogError("Multiple battle controllers!");
        singleton = this;

        _readyPlayers = new bool[upgradeControllers.Count];

        StartCombat();
    }

    public static void BecomeReady(UpgradeController player)
    {
        int index = singleton.upgradeControllers.FindIndex(X => X == player);
        _readyPlayers[index] = true;

        numReadyPlayers = 0;
        foreach (bool p in _readyPlayers)
        {
            if (p)
                numReadyPlayers += 1;
        }

        if (numReadyPlayers == _readyPlayers.Length)
            StartCombat();
    }

    public static void EndWave()
    {
        for (int i = 0; i < _readyPlayers.Length; ++i)
            _readyPlayers[i] = false;
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
        {
            upgrader.StartUpgrading();
        }
    }

    public static void StartCombat()
    {
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
        {
            upgrader.Hide();
        }
        singleton.enemySpawner.StartWave();
    }
}
