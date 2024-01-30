using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleController : MonoBehaviour
{
    public static BattleController singleton;

    public static int WaveNumber => singleton.enemySpawner.WaveNumber;
    public int stationHealth = 9;
    public SpriteRenderer spaceStation;
    public List<Sprite> stationHealthSprites = new List<Sprite>();
    public TorusTester tester;

    private static bool[] _readyPlayers;
    public static int numReadyPlayers = 0;

    public List<UpgradeController> upgradeControllers = new List<UpgradeController>();
    public RectTransform countdownPanel;
    public TMP_Text countdownText;
    public TMP_Text countdownTimer;
    public float countdownTime = 5f;

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
        {
            singleton.StartCountdown();
            //FinishUpgrading();
            //StartCombat();
        }
    }

    public static void BecomeNotReady(UpgradeController player)
    {
        int index = singleton.upgradeControllers.FindIndex(X => X == player);
        _readyPlayers[index] = false;

        if (singleton._countdownCoroutine != null)
            singleton.StopCountdown();
    }

    private void StartCountdown()
    {
        _countdownCoroutine = StartCoroutine(CountdownToStart());
    }

    private void StopCountdown()
    {
        StopCoroutine(_countdownCoroutine);
        countdownPanel.gameObject.SetActive(false);
        _countdownCoroutine = null;
    }

    private Coroutine _countdownCoroutine;
    private IEnumerator CountdownToStart()
    {
        countdownPanel.gameObject.SetActive(true);
        countdownText.text = "Wave " + WaveNumber + " starting in:";
        countdownTimer.text = countdownTime.ToString();

        float startTime = Time.realtimeSinceStartup;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (Time.realtimeSinceStartup < startTime + countdownTime)
        {
            countdownTimer.text = Utility.SecondsToTime(countdownTime - (Time.realtimeSinceStartup - startTime) - 1f);
            yield return wait;
        }

        FinishUpgrading();
        StartCombat();
        _countdownCoroutine = null;
    }

    public static void EndWave()
    {
        for (int i = 0; i < _readyPlayers.Length; ++i)
            _readyPlayers[i] = false;
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
        {
            upgrader.StartUpgrading();
        }

        PauseManager.SystemPause(singleton);
    }

    public static void StartCombat()
    {
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
        {
            upgrader.Hide();
        }
        singleton.countdownPanel.gameObject.SetActive(false);
        singleton.enemySpawner.StartWave();
        PauseManager.SystemUnPause(singleton);
    }

    public static void FinishUpgrading()
    {
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
        {
            upgrader.FinishUpgrading();
        }
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
