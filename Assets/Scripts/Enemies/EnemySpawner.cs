using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public MissionData missionData;

    [EnumNamedArray(typeof(EnemyClass))]
    public Enemy[] classBasePrefabs = new Enemy[Enum.GetNames(typeof(EnemyClass)).Length];
    public float spawningHeight = 10f;
    [EnumNamedArray(typeof(EnemyClass))]
    public float[] enemySpacing = new float[Enum.GetNames(typeof(EnemyClass)).Length];
    [Min(0.001f)]
    public float spacingRate = 0.001f;
    [Min(0.001f)]
    public float spacingForce = 0.5f;
    [EnumNamedArray(typeof(EnemyClass))]
    public FleetType[] defaultFleets = new FleetType[Enum.GetNames(typeof(EnemyClass)).Length];

    private int _waveNumber = 0;

    private List<EnemyFleet> fleetsToSpawn = new List<EnemyFleet>();
    private List<float> fleetAngles = new List<float>();

    [SerializeField]
    private List<Enemy> enemies = new List<Enemy>();

    public bool DoEasyTestWave = true;
    public List<EnemyClass> easyTestWave = new List<EnemyClass>();

    private float _lastSpacing = 0;

    public void StartWave()
    {
        if (DoEasyTestWave)
        { 
            EasyTestWave();
            return;
        }

        //Randomly assign difficulty points to fleets of main/rare enemies
        fleetAngles.Clear();
        ChooseFleets(missionData.missionPlan.waves[_waveNumber].possibleMainFleets, missionData.missionPlan.waves[_waveNumber].pointsForMainTypes);
        ChooseFleets(missionData.missionPlan.waves[_waveNumber].possibleRareFleets, missionData.missionPlan.waves[_waveNumber].pointsForRareTypes);
        fleetsToSpawn.ShuffleList();

        foreach (EnemyFleet fleet in fleetsToSpawn)
        {
            StartCoroutine(SpawnFleet(fleet, fleet.startDelay));
        }
    }

    private void ChooseMainFleets()
    {
        int points = missionData.missionPlan.waves[_waveNumber].pointsForMainTypes;
        List<FleetType> mainFleetTypes = missionData.missionPlan.waves[_waveNumber].possibleMainFleets;
        List<int> fleetTypeIndexes = Utility.CreateIndexList(mainFleetTypes.Count);

        while (points > 0)
        {
            FleetType plan;
            if (fleetTypeIndexes.Count == 0)
                plan = defaultFleets.Random();
            else
                plan = mainFleetTypes[fleetTypeIndexes.Random()];
            EnemyData randomMainType = missionData.mainEnemyTypes.Random();
            float angle = ChooseFleetAngle();
            fleetsToSpawn.Add(new EnemyFleet { baseAngle = angle, enemyType = randomMainType, plan = plan, startDelay = plan.difficultyPoints });
            points -= plan.difficultyPoints;

            //Remove fleet types that require more than the remaining points
            for (int i = 0; i < fleetTypeIndexes.Count; ++i)
            {
                if (mainFleetTypes[fleetTypeIndexes[i]].difficultyPoints > points)
                    fleetTypeIndexes.RemoveAt(i--);
            }
        }
    }
    private void ChooseRareFleets()
    {
        int points = missionData.missionPlan.waves[_waveNumber].pointsForRareTypes;
        List<FleetType> rareFleetTypes = missionData.missionPlan.waves[_waveNumber].possibleRareFleets;
        List<int> fleetTypeIndexes = Utility.CreateIndexList(rareFleetTypes.Count);

        while (points > 0)
        {
            FleetType plan;
            if (fleetTypeIndexes.Count == 0)
                plan = defaultFleets.Random();
            else
                plan = rareFleetTypes[fleetTypeIndexes.Random()];
            EnemyData randomMainType = missionData.mainEnemyTypes.Random();
            float angle = ChooseFleetAngle();
            fleetsToSpawn.Add(new EnemyFleet { baseAngle = angle, enemyType = randomMainType, plan = plan, startDelay = plan.difficultyPoints });
            points -= plan.difficultyPoints;

            //Remove fleet types that require more than the remaining points
            for (int i = 0; i < fleetTypeIndexes.Count; ++i)
            {
                if (rareFleetTypes[fleetTypeIndexes[i]].difficultyPoints > points)
                    fleetTypeIndexes.RemoveAt(i--);
            }
        }
    }
    private void ChooseFleets(List<FleetType> fleetOptions, int points)
    {
        List<int> fleetTypeIndexes = Utility.CreateIndexList(fleetOptions.Count);

        while (points > 0)
        {
            FleetType plan;
            if (fleetTypeIndexes.Count == 0)
                plan = defaultFleets.Random();
            else
                plan = fleetOptions[fleetTypeIndexes.Random()];
            EnemyData randomMainType = missionData.mainEnemyTypes.Random();
            float angle = ChooseFleetAngle();
            fleetsToSpawn.Add(new EnemyFleet { baseAngle = angle, enemyType = randomMainType, plan = plan, startDelay = plan.difficultyPoints });
            points -= plan.difficultyPoints;

            //Remove fleet types that require more than the remaining points
            for (int i = 0; i < fleetTypeIndexes.Count; ++i)
            {
                if (fleetOptions[fleetTypeIndexes[i]].difficultyPoints > points)
                    fleetTypeIndexes.RemoveAt(i--);
            }
        }
    }

    private float ChooseFleetAngle()
    {
        if (fleetAngles.Count == 0)
            return Random.Range(0f, 360f);

        float biggestGap = 0f;
        float prevAngle = fleetAngles.Last();
        int gapIndex = 0;
        for (int i = 0; i < fleetAngles.Count; ++i)
        {
            float gap = Mathf.Abs(fleetAngles[i] - fleetAngles[(i + 1) % fleetAngles.Count]);
            if (gap > biggestGap)
            {
                biggestGap = gap;
                gapIndex = i;
            }
        }
        float start = fleetAngles[gapIndex];
        float end = fleetAngles[(gapIndex + 1) % fleetAngles.Count];
        float angle = (Random.Range(start, end) + Random.Range(start, end)) / 2f;
        fleetAngles.Add(angle);
        return angle;
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
            EnemyData randomMainType = missionData.mainEnemyTypes.Random();
            EnemyClass randomClass = (EnemyClass)Random.Range(0, System.Enum.GetNames(typeof(EnemyClass)).Length);
            SpawnEnemy(i, randomMainType, randomClass);
        }
    }

    private void EasyTestWave()
    {
        EnemyData randomMainType = missionData.mainEnemyTypes.Random();

        foreach (EnemyClass enemyClass in easyTestWave)
        {
            SpawnEnemy(0, randomMainType, enemyClass);
            SpawnEnemy(180, randomMainType, enemyClass);
        }
    }

    private IEnumerator SpawnFleet(EnemyFleet fleet, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        List<Spawn> spawns = new List<Spawn>();
        foreach (FleetType.EnemyGroup group in fleet.plan.Groups)
        {
            for (int i = 0; i < group.numberOfEnemies; ++i)
                spawns.Add(new Spawn { angle = group.relativeAngle, enemyClass = group.enemyClass, delay = group.arrivalDelay });
        }
        spawns.Sort(CompareSpawns);

        float startTime = Time.time;
        float timeSinceStart = 0;
        float delay = 0;

        foreach (Spawn spawn in spawns)
        {
            timeSinceStart = Time.time - startTime;
            delay = spawn.delay - timeSinceStart;
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            SpawnEnemy(fleet.baseAngle + spawn.angle, fleet.enemyType, spawn.enemyClass);
        }

        fleetsToSpawn.Remove(fleet);
    }

    private void SpawnEnemy(float angle, EnemyData data, EnemyClass enemyClass)
    {
        Enemy newEnemy = Instantiate(classBasePrefabs[(int)enemyClass], transform);
        newEnemy.SetData(data);
        float height = spawningHeight;
        angle = angle + Random.Range(-2f, 2f);
        if (enemyClass == EnemyClass.swarm)
        {
            height += Random.Range(0, 0.5f);
            if (Random.value < data.Ability(EnemyClass.swarm))
                SpawnExtraSwarmEnemy(angle, data);
        }
        newEnemy.AngleAndHeight = new Vector2(angle, height);
        enemies.Add(newEnemy);
        newEnemy.destroyEvents += EnemyDestroyed;
    }

    private void SpawnExtraSwarmEnemy(float originAngle, EnemyData data)
    {
        Enemy newEnemy = Instantiate(classBasePrefabs[(int)EnemyClass.swarm], transform);
        newEnemy.SetData(data);
        float height = spawningHeight;
        newEnemy.AngleAndHeight = new Vector2(originAngle + 1f, spawningHeight + Random.Range(0, 0.5f));
        enemies.Add(newEnemy);
        newEnemy.destroyEvents += EnemyDestroyed;
    }

    public void EnemyDestroyed(Enemy enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0 && fleetsToSpawn.Count == 0)
            BattleController.EndWave();
    }

    public struct EnemyFleet
    {
        public float startDelay;
        public FleetType plan;
        public EnemyData enemyType;
        public float baseAngle;
    }

    private struct Spawn
    {
        public float angle;
        public EnemyClass enemyClass;
        public float delay;
    }

    private int CompareSpawns(Spawn A, Spawn B)
    {
        if (A.delay > B.delay)
            return 1;
        if (A.delay < B.delay)
            return -1;
        return 0;
    }

    #region Enemy Spacing
    private void FixedUpdate()
    {
        if (Time.time > _lastSpacing + spacingRate)
            SpaceEnemies();
    }

    private float[] _enemyDists = new float[5];
    private float _dist = 0;
    private float _tempDist = 0;
    private Vector2 _diff;
    private Vector2 _spacingDir = Vector2.zero;
    private float _enemySize = 0;
    private void SpaceEnemies()
    {
        _lastSpacing = Time.time;

        //Loop through each enemy
        foreach (Enemy enemy in enemies)
        {
            //Reset values
            _spacingDir = Vector2.zero;
            for (int i = 0; i < _enemyDists.Length; ++i)
                _enemyDists[i] = float.PositiveInfinity;
            _enemySize = enemySpacing[(int)enemy.myClass];

            //Loop through all other enemies
            foreach (Enemy otherEnemy in enemies)
            {
                if (enemy == otherEnemy)
                    continue;

                //Calculate the square distance between enemies
                _diff = enemy.transform.position - otherEnemy.transform.position;
                if (_diff == Vector2.zero)
                    _diff = Random.insideUnitCircle;
                float dist = Vector2.SqrMagnitude(_diff);

                //Calculate spacing force
                if (otherEnemy.myClass == EnemyClass.tank || enemy.myClass != EnemyClass.tank)
                {
                    //Add spacing
                    float maxSpacingDist = _enemySize + enemySpacing[(int)otherEnemy.myClass];
                    if (dist < maxSpacingDist)
                    {
                        float inverter = (maxSpacingDist - dist) / (maxSpacingDist / 2);
                        _spacingDir += (_diff * inverter * inverter);
                    }
                }

                //Place other enemy in nearby enemies list (contains 5 nearest enemies sorted from closest to furthest)
                for (int i = 0; i < _enemyDists.Length; ++i)
                {
                    if (dist < _enemyDists[i])
                    {
                        enemy.nearbyEnemies[i] = otherEnemy;
                        _tempDist = _enemyDists[i];
                        _enemyDists[i] = dist;
                        dist = _tempDist;
                    }
                }

            }

            _spacingDir = enemy.VectorAsAngleHeight((_spacingDir * spacingForce * spacingRate) / _enemySize);
            _spacingDir.y *= 0.8f;
            enemy.AngleAndHeight += _spacingDir;
        }
    }
    #endregion
}
