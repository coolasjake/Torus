using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Basic Settings")]
    [Min(1f)]
    [SerializeField]
    private float health = 10f;
    /// <summary> Armour reduces hit value (i.e. before any other effects are calculated) of all damage types except basic by 5% per level.
    /// Can be reduced by effects, most commonly from acid. </summary>
    [Range(0, 10)]
    [SerializeField]
    private int armourLevel = 0;
    [Min(1)]
    [SerializeField]
    private float speed = 1f;

    [Header("Damage Settings")]
    /// <summary> Reduces damage based on values (0 = full damage, 1 = no damage, -1 = double damage). DOT effects have damage reduced, not application reduced.  </summary>    
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

    [SerializeField]
    private List<EnemyClassData> classes = new List<EnemyClassData> {
        new EnemyClassData(EnemyClass.swarm), new EnemyClassData(EnemyClass.fast),
        new EnemyClassData(EnemyClass.dodge), new EnemyClassData(EnemyClass.tank)};

    public int Points(EnemyClass enemyClass) => classes[(int)enemyClass].pointsCost;
    public float Health(EnemyClass enemyClass) => health * classes[(int)enemyClass].healthMultiplier;
    public int Armour(EnemyClass enemyClass) => armourLevel + classes[(int)enemyClass].extraArmour;
    public float Speed(EnemyClass enemyClass) => speed * classes[(int)enemyClass].speedMultiplier;
    public float Ability(EnemyClass enemyClass) => classes[(int)enemyClass].abilityPower;
    public Sprite ClassSprite(EnemyClass enemyClass) => classes[(int)enemyClass].sprite;
    public AnimatorController ClassAnimations(EnemyClass enemyClass) => classes[(int)enemyClass].animationController;
    public float Size(EnemyClass enemyClass) => classes[(int)enemyClass].size;

    [Header("Effect Prefabs")]
    public GameObject explosionPrefab;

    [System.Serializable]
    public class EnemyClassData
    {
        public EnemyClassData(EnemyClass enemyClass)
        {
            name = enemyClass.ToString();
            className = name;

            if (enemyClass == EnemyClass.tank)
            {
                pointsCost = 10;
                healthMultiplier = 5f;
                extraArmour = 3;
                speedMultiplier = 0.5f;
                abilityPower = 1f;
                size = 5f;
            }
            else if (enemyClass == EnemyClass.fast)
            {
                pointsCost = 5;
                healthMultiplier = 2f;
                extraArmour = 0;
                speedMultiplier = 1.2f;
                abilityPower = 1.5f;
                size = 2f;
            }
            else if (enemyClass == EnemyClass.dodge)
            {
                pointsCost = 5;
                healthMultiplier = 3f;
                extraArmour = 1;
                speedMultiplier = 1f;
                abilityPower = 10f;
                size = 2f;
            }
        }

        [HideInInspector]
        public string name = "";
        public string className = "";
        [Min(1)]
        public int pointsCost = 1;
        [Min(0.001f)]
        public float healthMultiplier = 1f;
        [Min(0)]
        public int extraArmour = 0;
        [Min(0.1f)]
        public float speedMultiplier = 1f;
        [Tooltip("Tank = Damage reduction per hit/type, Dodge = time between dodges, Fast = speed multiplier (when not stunned)")]
        ///<summary> Tank = Damage reduction per hit/type, Dodge = time between dodges, Fast = speed multiplier (when not stunned). </summary>
        public float abilityPower = 0f;
        public float size = 1f;

        public Sprite sprite;
        public AnimatorController animationController;
    }
}


[System.Serializable]
public class AllDamage
{
    [EnumNamedArray(typeof(DamageType))]
    public float[] damageTypes = new float[Enum.GetNames(typeof(DamageType)).Length];

    public void SetDamage(DamageType type, float value)
    {
        damageTypes[(int)type] = value;
    }

    public float GetDamage(DamageType type)
    {
        return damageTypes[(int)type];
    }

    public float Basic => damageTypes[(int)DamageType.basic];
    public float Physical => damageTypes[(int)DamageType.physical];
    public float Heat => damageTypes[(int)DamageType.heat];
    public float Cold => damageTypes[(int)DamageType.cold];
    public float Radiation => damageTypes[(int)DamageType.radiation];
    public float Acid => damageTypes[(int)DamageType.acid];
    public float Lightning => damageTypes[(int)DamageType.lightning];
    public float Nanites => damageTypes[(int)DamageType.nanites];
    public float Antimatter => damageTypes[(int)DamageType.antimatter];
}