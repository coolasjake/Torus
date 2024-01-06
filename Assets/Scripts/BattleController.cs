using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public static BattleController singleton;

    public int stationHealth = 9;
    public SpriteRenderer spaceStation;
    public List<Sprite> stationHealthSprites = new List<Sprite>();
    public TorusTester tester;

    private static bool[] _readyPlayers;
    public static int numReadyPlayers = 0;

    public List<UpgradeController> upgradeControllers = new List<UpgradeController>();
    public EnemySpawner enemySpawner;

    public bool getAbilityOnStart = false;

    void Start()
    {
        if (singleton != null)
            Debug.LogError("Multiple battle controllers!");
        singleton = this;

        _readyPlayers = new bool[upgradeControllers.Count];

        ShowStationDamage();
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
            upgrader.Initialize();

        if (getAbilityOnStart)
            EndWave();
        else
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
        if (singleton.stationHealth <= 0)
            GameOver();
        else
        {
            singleton.stationHealth -= damage;
            singleton.ShowStationDamage();
            float explosionHeight = ((damage / 5f) * (singleton.enemySpawner.spawningHeight - 1f)) + 1f;
            singleton.enemySpawner.ExplodeEnemies(explosionHeight);
            StaticRefs.SpawnStationExplosion(explosionHeight * 10f);
            singleton.tester.gizmoHeight = explosionHeight;
        }
    }

    public static void GameOver()
    {

    }

    private void ShowStationDamage()
    {
        spaceStation.sprite = stationHealthSprites[Mathf.Clamp((stationHealth / 3) - 1, 0, stationHealthSprites.Count - 1)];
    }
}
