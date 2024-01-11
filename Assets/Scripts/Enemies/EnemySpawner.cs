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
    [Min(0.0001f)]
    public float maxSpacingMove = 0.01f;
    [EnumNamedArray(typeof(EnemyClass))]
    public FleetType[] defaultFleets = new FleetType[Enum.GetNames(typeof(EnemyClass)).Length];

    public int WaveNumber => _waveNumber;
    private int _waveNumber = 0;
    private bool _startSpawning = false;
    private float _waveStartTime = 0;

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
        if (_waveNumber >= missionData.missionPlan.waves.Count)
            ChooseAllFleets(missionData.missionPlan.waves[missionData.missionPlan.waves.Count - 1]);
        else
            ChooseAllFleets(missionData.missionPlan.waves[_waveNumber]);

        string waveDescription = "Wave " + _waveNumber + " (" + missionData.missionPlan.waves[_waveNumber].name + ") = ";
        foreach (EnemyFleet fleet in fleetsToSpawn)
        {
            StartCoroutine(SpawnFleet(fleet, fleet.startDelay));
            waveDescription += "\n" + fleet.plan.name + " of " + fleet.enemyType.name + ", starting at " + fleet.startDelay;
        }
        Debug.Log(waveDescription);

        _startSpawning = true;
        _waveStartTime = Time.time;
    }

    private void ChooseAllFleets(MissionPlan.WaveData wave)
    {
        float[] armyAngles = new float[missionData.mainEnemyTypes.Count];
        armyAngles[0] = Random.Range(0f, 360f);
        float armySpacing = 360f / armyAngles.Length;
        for (int i = 1; i < armyAngles.Length; ++i)
            armyAngles[i] = (armyAngles[0] + armySpacing + Random.Range(-armySpacing * 0.2f, armySpacing * 0.2f)) % 360f;
        armyAngles.Shuffle();
        armySpacing = Mathf.Min(armySpacing, 120f);

        List<int> mainTypeIndexes = Utility.CreateIndexList(wave.fleets.Count);
        List<int> rareTypeIndexes = Utility.CreateIndexList(wave.fleets.Count);
        int mPoints = 0;
        int rPoints = 0;
        int totalPoints = wave.pointsForMainTypes + wave.pointsForRareTypes;
        int currentBurst = 0;

        int rareStartingPoint = Mathf.RoundToInt(wave.pointsForMainTypes * wave.rareTypesStartTime);
        int rareSectionSize = Mathf.CeilToInt((totalPoints - rareStartingPoint) * wave.rareTypesEndTime);

        for (int i = 0; i < mainTypeIndexes.Count; ++i)
        {
            if (wave.fleets[mainTypeIndexes[i]].difficultyPoints > (wave.pointsForMainTypes - mPoints))
                mainTypeIndexes.RemoveAt(i--);
        }
        for (int i = 0; i < rareTypeIndexes.Count; ++i)
        {
            if (wave.fleets[rareTypeIndexes[i]].difficultyPoints > (wave.pointsForRareTypes - mPoints))
                rareTypeIndexes.RemoveAt(i--);
        }

        int fleetNum = 0;
        int randomOffset = Random.Range(0, wave.fleets.Count);
        while (mPoints + rPoints < totalPoints)
        {
            //Choose between a Main or Rare fleet
            bool spawningMain = true;
            if (mPoints + rPoints >= rareStartingPoint)
            {
                if (rPoints >= wave.pointsForRareTypes)
                    //If rares have used more than their allowed points, spawn a main
                    spawningMain = true;
                else if (mPoints + rPoints >= rareStartingPoint + rareSectionSize)
                    //If the total points is greater than the end of the rare section, spawn a rare (also stops /0 errors)
                    spawningMain = false;
                else if ((wave.pointsForRareTypes - rPoints) / (float)wave.pointsForRareTypes >=
                    (rareSectionSize - (rPoints + mPoints - rareStartingPoint)) / rareSectionSize)
                    //If the % of rares remaining is greater than the % of points remaining in the rare section, spawn a rare
                    spawningMain = false;
                //Else, spawn a main
            }

            //Choose a fleet plan
            FleetType plan;
            if ((spawningMain ? mainTypeIndexes.Count : rareTypeIndexes.Count) == 0)
                plan = defaultFleets.Random();
            else if (wave.pickFleetsRandomly)
                plan = wave.fleets[(spawningMain ? mainTypeIndexes : rareTypeIndexes).Random()];
            else
            {
                if (spawningMain)
                    plan = wave.fleets[mainTypeIndexes[(fleetNum + randomOffset) % mainTypeIndexes.Count]];
                else
                    plan = wave.fleets[rareTypeIndexes[(fleetNum + randomOffset) % rareTypeIndexes.Count]];
            }

            //Choose an enemy type
            EnemyData chosenType;
            int chosenIndex = 0;
            if (spawningMain)
            {
                chosenIndex = Random.Range(0, missionData.mainEnemyTypes.Count);
                chosenType = missionData.mainEnemyTypes[chosenIndex];
            }
            else
                chosenType = missionData.rareEnemyTypes.Random();

            //Choose angle and start time, then add the fleet to the list
            float angle;
            if (spawningMain)
                angle = ChooseFleetAngle(armyAngles[chosenIndex], armySpacing);
            else
                angle = ChooseFleetAngle(Random.Range(0f, 360f), 360f);
            float startTime = ((float)currentBurst / wave.numBursts) * wave.waveTime;
            fleetsToSpawn.Add(new EnemyFleet { baseAngle = angle, enemyType = chosenType, plan = plan, startDelay = startTime });
            ++fleetNum;
            if (spawningMain)
                mPoints += plan.difficultyPoints;
            else
                rPoints += plan.difficultyPoints;

            currentBurst = (mPoints + rPoints) / (totalPoints / wave.numBursts);


            //Remove fleet types that require more than the remaining points
            for (int i = 0; i < mainTypeIndexes.Count; ++i)
            {
                if (wave.fleets[mainTypeIndexes[i]].difficultyPoints > (wave.pointsForMainTypes - mPoints))
                    mainTypeIndexes.RemoveAt(i--);
            }
            for (int i = 0; i < rareTypeIndexes.Count; ++i)
            {
                if (wave.fleets[rareTypeIndexes[i]].difficultyPoints > (wave.pointsForRareTypes - mPoints))
                    rareTypeIndexes.RemoveAt(i--);
            }
        }
    }

    private float ChooseFleetAngle(float armyAngle, float armySpacing)
    {
        float spacing = armySpacing / 2f;
        if (fleetAngles.Count == 0)
            return Random.Range(armyAngle - spacing, armyAngle + spacing);

        float biggestGap = 0f;
        float prevAngle = fleetAngles.Last();
        int gapIndex = 0;
        //Find pair of angles that have the biggest gap between them
        for (int i = 0; i < fleetAngles.Count; ++i)
        {
            float angleA = Mathf.Clamp(TorusMotion.SignedAngle(armyAngle, fleetAngles[i]), -spacing, spacing) + armyAngle;
            float angleB = Mathf.Clamp(TorusMotion.SignedAngle(armyAngle, fleetAngles[(i + 1) % fleetAngles.Count]), -spacing, spacing) + armyAngle;
            float gap = Mathf.Abs(angleA - angleB);
            if (gap > biggestGap)
            {
                biggestGap = gap;
                gapIndex = i;
            }
        }
        float start = fleetAngles[gapIndex];
        float end = fleetAngles[(gapIndex + 1) % fleetAngles.Count];
        float angle = (Random.Range(start, end) + Random.Range(start, end)) / 2f;
        angle = Mathf.Clamp(TorusMotion.SignedAngle(armyAngle, angle), -spacing, spacing) + armyAngle;
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
            //height += Random.Range(0, 0.5f);
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
            EndWave();
    }

    private void EndWave()
    {
        _waveNumber += 1;
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

    public void ExplodeEnemies(float height)
    {
        List<Enemy> enemiesToDestroy = new List<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy.Height < height)
            {
                enemiesToDestroy.Add(enemy);
            }
        }
        foreach (Enemy enemy in enemiesToDestroy)
        {
            enemy.Destroy();
        }
    }

    #region Enemy Spacing
    private void FixedUpdate()
    {
        if (Time.time > _lastSpacing + spacingRate)
            SpaceEnemies();
    }

    private float[] _enemyDists = new float[5];
    private float _tempDist = 0;
    private Vector2 _diff;
    private Vector2 _spacingDir = Vector2.zero;
    private float _enemySize = 0;
    private void SpaceEnemies()
    {
        for(int i = 0; i < enemySpacing.Length; ++i)
        {
            if (enemySpacing[i] <= 0)
            {
                Debug.LogError("Spacings cannot go below 0!");
                enemySpacing[i] = 0.001f;
            }
        }
        _lastSpacing = Time.time;

        //Loop through each enemy
        foreach (Enemy enemy in enemies)
        {
            //Reset values
            _spacingDir = Vector2.zero;
            for (int i = 0; i < _enemyDists.Length; ++i)
                _enemyDists[i] = float.PositiveInfinity;
            _enemySize = enemySpacing[(int)enemy.Class];

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
                if (otherEnemy.Class == EnemyClass.tank || enemy.Class != EnemyClass.tank)
                {
                    //Add spacing
                    float maxSpacingDist = _enemySize + enemySpacing[(int)otherEnemy.Class];
                    if (dist < maxSpacingDist * maxSpacingDist)
                    {
                        Vector2 spacing = _diff / (dist / maxSpacingDist);
                        spacing = spacing - spacing.normalized;
                        _spacingDir += spacing;
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

            //Apply rates and limits
            _spacingDir = _spacingDir * (spacingForce * spacingRate);
            if (_spacingDir.SqrMagnitude() > maxSpacingMove * maxSpacingMove)
                _spacingDir = _spacingDir.normalized * maxSpacingMove;

            //Convert to angle/height, then reduce height element (so enemies space out around instead of away/towards station)
            _spacingDir = enemy.VectorAsAngleHeight(_spacingDir);
            _spacingDir.y *= 0.8f;

            //Apply to enemy
            enemy.AngleAndHeight += _spacingDir;
        }
        CheckReachedStation();
        if (breakAfterSpacing)
            Debug.Break();
    }

    public void CheckReachedStation()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.Height <= 1 + (enemySpacing[(int)enemy.Class] * 0.5f))
            {
                BattleController.DamageStation((int)enemy.Size);
                break;
            }
        }
    }

    public bool breakAfterSpacing = false;
    private void OnDrawGizmosSelected()
    {
        TorusTester.DrawTorusGizmo(spawningHeight, 180, Color.red);

        for (int i = 0; i < Mathf.Min(enemies.Count); ++i)
        {
            Gizmos.color = Color.red.ChangeHue((i * 256f) / enemies.Count);
            Gizmos.DrawWireSphere(enemies[i].transform.position, enemySpacing[(int)enemies[i].Class]);
        }
    }
    #endregion
}
