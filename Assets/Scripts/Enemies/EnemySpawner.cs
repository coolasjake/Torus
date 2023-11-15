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

    private List<EnemyFleet> fleets = new List<EnemyFleet>();

    void Start()
    {
        for(int i = 0; i < 360; i += 20)
        {
            EnemyData randomMainType = missionData.mainEnemyTypes.Rand();
            EnemyClass randomClass = (EnemyClass)Random.Range(0, System.Enum.GetNames(typeof(EnemyClass)).Length);
            SpawnEnemy(i, randomMainType, randomClass);
        }
    }

    public void StartWave()
    {

    }

    private void PlanWave()
    {

    }

    private void SpawnEnemy(float angle, EnemyData data, EnemyClass enemyClass)
    {
        Enemy newEnemy = Instantiate(classBasePrefabs[(int)enemyClass], transform);
        newEnemy.SetData(data);
        newEnemy.AngleAndHeight = new Vector2(angle, spawningHeight);
    }

    private class EnemyFleet
    {
        public float startTime = 0f;
        public List<EnemyData> enemies = new List<EnemyData>();
        public List<EnemyClass> types = new List<EnemyClass>();
        public float angle = 0f;
    }
}
