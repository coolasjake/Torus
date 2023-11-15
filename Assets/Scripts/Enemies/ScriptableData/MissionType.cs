using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DefaultMission", menuName = "ScriptableObjects/MissionType", order = 1)]
public class MissionType : ScriptableObject
{
    public List<WaveData> waves = new List<WaveData>();
}

[System.Serializable]
public class WaveData()
{
    public bool difficultyMultipliesNumber;
    public bool difficultyMultipliesStats;
    [Min(1)]
    public int numMainTypes = 1;
    [Min(1)]
    public int baseNumMainEnemies = 10;
    [Min(1f)]
    public float diffNumMultMain = 1f;
    [Min(1f)]
    public float diffStatsMultMain = 1f;
    [EnumNamedArray(typeof(EnemyType))]
    public bool[] allowedTypes = new bool[Enum.GetNames(typeof(EnemyType)).Length];

    [Min(0)]
    public int numRareTypes = 0;
    [Min(0)]
    public int baseNumRareEnemies = 0;
    [Min(1f)]
    public float diffNumMultRare = 1f;
    [Min(1f)]
    public float diffStatsMultRare = 1f;

}