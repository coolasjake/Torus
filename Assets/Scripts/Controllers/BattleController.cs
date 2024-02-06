using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public static BattleController singleton;
    public static MissionData missionData;

    public MissionData testMissionData;

    public static int WaveNumber => singleton.enemySpawner.WaveNumber;
    public int stationHealth = 9;
    public SpriteRenderer spaceStation;
    public List<Sprite> stationHealthSprites = new List<Sprite>();
    public TorusTester tester;

    private static bool[] _readyPlayers;
    public static int numReadyPlayers = 0;

    public List<UpgradeController> upgradeControllers = new List<UpgradeController>();

    public EnemySpawner enemySpawner;

    public bool getAbilityOnStart = false;

    [Header("Upgrade/Pause Countdown")]
    public RectTransform countdownPanel;
    public TMP_Text countdownText;
    public TMP_Text countdownTimer;
    public float countdownTime = 5f;

    [Header("Gameover")]
    public RectTransform gameoverPanel;
    public float preGameoverTime = 5f;
    public float gameoverTime = 10f;
    public string hubScene = "Hub";

    void Start()
    {
        //Setup Singleton
        if (singleton != null)
            Debug.LogError("Multiple battle controllers!");
        singleton = this;

        //Set mission to default if null for testing
        if (missionData == null)
            missionData = testMissionData;

        //Setup array of player ready-states
        _readyPlayers = new bool[upgradeControllers.Count];

        //Setup station health visuals
        ShowStationDamage();

        //Spawn weapons and initialize UpgradeControllers
        int count = Mathf.Min(singleton.upgradeControllers.Count, missionData.chosenWeapons.Count);
        for (int i = 0; i < count; ++i)
        {
            Weapon weapon = StaticRefs.SpawnWeapon(missionData.chosenWeapons[i], i);
            singleton.upgradeControllers[i].targetWeapon = weapon;
        }
        foreach (UpgradeController upgrader in singleton.upgradeControllers)
            upgrader.Initialize();

        //Hide gameover panel
        gameoverPanel.gameObject.SetActive(false);

        //Start the first wave or upgrade
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

    private IEnumerator ShowGameoverThenGoToMenu()
    {
        yield return new WaitForSecondsRealtime(preGameoverTime);
        gameoverPanel.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(gameoverTime);
        SceneManager.LoadScene(hubScene);
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
            singleton.GameOver();
        else
        {
            singleton.stationHealth -= damage;
            singleton.ShowStationDamage();
            float explosionHeight = ((damage / 5f) * (StaticRefs.SpawningHeight - 1f)) + 1f;
            singleton.enemySpawner.ExplodeEnemies(explosionHeight);
            StaticRefs.SpawnStationExplosion(explosionHeight * 10f);
            singleton.tester.gizmoHeight = explosionHeight;
        }
    }

    public void GameOver()
    {
        StartCoroutine(ShowGameoverThenGoToMenu());
    }

    private void ShowStationDamage()
    {
        spaceStation.sprite = stationHealthSprites[Mathf.Clamp((stationHealth / 3) - 1, 0, stationHealthSprites.Count - 1)];
    }
}
