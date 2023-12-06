using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public static BattleController singleton;

    public int stationHealth = 9;
    public SpriteRenderer spaceStation;
    public List<Sprite> stationHealthSprites = new List<Sprite>();

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

        ShowStationDamage();

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

    public static void DamageStation(int damage)
    {
        singleton.stationHealth -= damage;
        if (singleton.stationHealth <= 0)
            GameOver();
        else
            singleton.ShowStationDamage();
    }

    private static void StationExplosion(int size)
    {

    }

    public static void GameOver()
    {

    }

    private void ShowStationDamage()
    {
        spaceStation.sprite = stationHealthSprites[Mathf.Clamp((stationHealth / 3) - 1, 0, stationHealthSprites.Count - 1)];
    }
}
