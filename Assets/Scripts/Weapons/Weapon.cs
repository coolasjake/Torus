using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class Weapon : TorusMotion
{
    [Header("Weapon Options")]
    public int weaponIndex = 0;

    public ModifiableFloat moveSpeed = new ModifiableFloat(10f);
    public ModifiableFloat aimingMult = new ModifiableFloat(0.5f, 0f, 1f);

    public ModifiableFloat fireRate = new ModifiableFloat(1f, 0.0001f, 60f);

    public AllDamageStats damageStats = new AllDamageStats();

    protected float _lastShot = 0f;

    public SpriteRenderer weaponRenderer;

    public GameObject attackPrefab;

    public Transform firingPoint;

    public List<Ability> unlockedAbilities = new List<Ability>();

    private float experience = 0;
    private float experienceNeeded = 100f;
    private int level = 0;
    private int upgradePoints = 0;

    /*
    public delegate void EnemyHitEvent(Weapon origin, Enemy target);
    public EnemyHitEvent EnemyHit;

    public delegate void WeaponFiredEvent(Weapon origin);
    public WeaponFiredEvent WeaponFired;
    */

    void Start()
    {
        Setup();
    }

    protected bool _firing = false;
    protected float _actualMoveSpeed;

    void FixedUpdate()
    {
        if (PauseManager.Paused)
            return;

        _actualMoveSpeed = moveSpeed.Value;

        //Shoot
        if (Input.GetButton("Fire" + weaponIndex))
        {
            _firing = true;
            _actualMoveSpeed = moveSpeed.Value * aimingMult.Value;
            Fire();
        }
        else
            _firing = false;

        //Move
        float input = Input.GetAxis("Horizontal" + weaponIndex);
        if (input > 0)
        {
            MoveAround(-_actualMoveSpeed * Time.fixedDeltaTime);
        }
        if (input < 0)
        {
            MoveAround(_actualMoveSpeed * Time.fixedDeltaTime);
        }


    }

    protected abstract void Setup();

    protected abstract void Fire();

    public virtual void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "movespeed":
                moveSpeed.AddModifier(modifierName, value, operation);
                break;
            case "aimingmult":
                aimingMult.AddModifier(modifierName, value, operation);
                break;
            case "firerate":
                fireRate.AddModifier(modifierName, value, operation);
                break;
        }
    }

    public void DefaultHit(Enemy enemy)
    {
        //Basic Damage
        enemy.health -= damageStats.basic.Value;

        //Physical Damage
        float physicalDamage = damageStats.physical.Value;
        if (enemy.frozen)
            physicalDamage *= 2f;
        physicalDamage *= (1f - enemy.data.resistances.Physical);
        enemy.health -= physicalDamage;

        //Heat Damage
        enemy.temperature += damageStats.heat.Value * (1f - enemy.data.resistances.Heat);

        if (enemy.health <= 0)
            KillEnemy(enemy);
        else
            enemy.lastHitBy = this;
    }

    public void KillEnemy(Enemy enemy)
    {
        experience += enemy.XPReward;
        if (experience > experienceNeeded)
            experience -= experienceNeeded;
        level += 1;
        upgradePoints += 1;

        enemy.SpawnExplosion();

        Destroy(enemy.gameObject);
        enemy.gameObject.SetActive(false);
    }
}

public enum WeaponType
{
    Any,
    MachineGun,
    Railgun,
    FlameThrower,
    Laser,
    MissileLauncher,
    FreezeRay,
    BoomerangChainsaw,
    Antimatter
}

public enum DamageType
{
    basic,      //default damage, uneffected by resistances or armor
    physical,   //kinetic damage. Applied instantly, heavily effected by armor, deals bonus damage to frozen
    heat,       //temperature change. Enemy freezes when low enough, and takes heat damage when high enough
    lightning,  //splits some of the damage to other nearby enemies based on conductivity value
    poison,     //add poison to target, target takes slow damage over time, often completely resisted
    acid,       //add acid to target, target takes quick damage over time, value reduces each time
    nanites,    //add nanites to target, target takes damage over time that goes down when their health gets lower and does nothing when below 10%.
    antimatter  //add antimatter to target, target explodes dealing basic damage to self and nearby enemies when hit by physical, poison, acid or nanites.
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
    public float Poison => damageTypes[(int)DamageType.poison];
    public float Acid => damageTypes[(int)DamageType.acid];
    public float Lightning => damageTypes[(int)DamageType.lightning];
    public float Nanites => damageTypes[(int)DamageType.nanites];
    public float Antimatter => damageTypes[(int)DamageType.antimatter];
}

[System.Serializable]
public class AllDamageStats
{
    public ModifiableFloat basic = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat physical = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat heat = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat poison = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat acid = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat lightning = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat nanites = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat antimatter = new ModifiableFloat(0, 0, float.PositiveInfinity);
}