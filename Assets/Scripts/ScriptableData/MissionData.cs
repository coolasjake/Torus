using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "ScriptableObjects/MissionData", order = 1)]
public class MissionData : ScriptableObject
{
    [Min(1)]
    public int difficultLevel = 1;

    public MissionType missionType;

    public List<EnemyData> mainEnemyTypes = new List<EnemyData>();
    public List<EnemyData> rareEnemyTypes = new List<EnemyData>();

    public List<WeaponType> chosenWeapons = new List<WeaponType>();
    public GameObject torusPrefab;

    public Sprite systemBackground;
}
