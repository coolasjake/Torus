using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public MissionData missionData;

    [EnumNamedArray(typeof(EnemyClass))]
    public Enemy[] classBasePrefabs = new Enemy[System.Enum.GetNames(typeof(EnemyClass)).Length];
    public float spawningHeight = 10f;

    private List<EnemyFleet> fleetsToSpawn = new List<EnemyFleet>();

    private List<Enemy> enemies = new List<Enemy>();

    public bool DoEasyTestWave = true;

    public void StartWave()
    {
        if (DoEasyTestWave)
            EasyTestWave();
        else
        {
            StartCoroutine(SpawnTestWaveDelayed(0f));
            StartCoroutine(SpawnTestWaveDelayed(10f));
            StartCoroutine(SpawnTestWaveDelayed(15f));
            StartCoroutine(SpawnTestWaveDelayed(18f));
        }
    }

    private IEnumerator SpawnTestWaveDelayed(float time)
    {
        yield return new WaitForSeconds(time);
        TestWave();
    }

    private void TestWave()
    {
        for (int i = 0; i < 360; i += 20)
        {
            EnemyData randomMainType = missionData.mainEnemyTypes.Rand();
            EnemyClass randomClass = (EnemyClass)Random.Range(0, System.Enum.GetNames(typeof(EnemyClass)).Length);
            SpawnEnemy(i, randomMainType, randomClass);
        }
    }

    private void EasyTestWave()
    {
        EnemyData randomMainType = missionData.mainEnemyTypes.Rand();
        SpawnEnemy(0, randomMainType, EnemyClass.fast);
        SpawnEnemy(180, randomMainType, EnemyClass.fast);
    }

    private void PlanWave()
    {

    }

    private void SpawnEnemy(float angle, EnemyData data, EnemyClass enemyClass)
    {
        Enemy newEnemy = Instantiate(classBasePrefabs[(int)enemyClass], transform);
        newEnemy.SetData(data);
        newEnemy.AngleAndHeight = new Vector2(angle, spawningHeight);
        enemies.Add(newEnemy);
        newEnemy.destroyEvents += EnemyDestroyed;
    }

    public void EnemyDestroyed(Enemy enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0 && fleetsToSpawn.Count == 0)
            BattleController.EndWave();
    }

    private class EnemyFleet
    {
        public float startTime = 0f;
        public List<EnemyData> enemies = new List<EnemyData>();
        public List<EnemyClass> types = new List<EnemyClass>();
        public float angle = 0f;
    }
}
