using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllEnemySettings", menuName = "ScriptableObjects/EnemySettings", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Settings")]
    [Min(1f)]
    public float health = 10f;
    [Range(0, 10)]
    public int armourLevel = 0;
    [Min(1)]
    public float speed = 1f;
    [Min(0)]
    public float XPReward = 0;

    [Header("Damage Settings")]
    /// <summary> Reduces damage based on values (0 = full damage, 1 = no damage). DOT effects have damage reduced, not application reduced.  </summary>
    public AllDamage resistances = new AllDamage();

    //Do multipliers for different enemy types

    /// <summary> Start taking heat damage when temp is above this value. </summary>
    public float maxSafeTemp = 100f;
    /// <summary> Temperature will change towards this value over time. </summary>
    public float restingTemp = 35f;
    /// <summary> Become frozen when temp is less than this value, and move slower when closer to this value (starting at zero). </summary>
    [Range(-1000f, -1f)]
    public float freezeTemp = -30f;

    public bool damageFromHot = true;
    public bool damageFromCold = false;

    //Static Values
    public float maxSlow = 0.8f;
    public float maxColdSlow = 0.2f;
    public float frozenSlow = 0.2f;
    public float baseTempChange = 1f;

    /// <summary> Percentage of lightning damage that is taken or conducted to nearby enemies (100 = all damage is conducted). </summary>
    [Range(0f, 100f)]
    public float conductivity = 50f;

    [Header("Effect Prefabs")]
    public GameObject explosionPrefab;
}
