using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "ScriptableObjects/MissionData", order = 1)]
public class MissionData : ScriptableObject
{
    public Faction faction = Faction.None;

    [Min(1)]
    public int difficulty = 1;

    //public MissionPlan missionPlan;
    public List<WaveData> waves = new List<WaveData>();

    public List<EnemyData> mainEnemyTypes = new List<EnemyData>();
    public List<EnemyData> rareEnemyTypes = new List<EnemyData>();

    public List<WeaponType> chosenWeapons = new List<WeaponType>();
    public GameObject torusPrefab;

    public Sprite systemBackground;
}

[System.Serializable]
public class WaveData
{
    public string name = "";
    public float waveTime = 180f;
    [Min(1)]
    public int pointsForMainTypes = 1;
    [Min(0)]
    public int pointsForRareTypes = 0;
    [Range(0f, 1f)]
    public float rareTypesStartTime = 0.0f;
    [Range(0f, 1f)]
    public float rareTypesEndTime = 1f;
    [Min(1)]
    public int numBursts = 3;
    [Range(0f, 180f)]
    public float maxSpread = 0;
    public bool pickFleetsRandomly = true;
    public List<FleetType> fleets = new List<FleetType>();
}