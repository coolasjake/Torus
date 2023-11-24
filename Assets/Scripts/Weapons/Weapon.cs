using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public abstract class Weapon : TorusMotion
{
    public abstract WeaponType Type();

    public int playerIndex = 0;
    public GameObject inputPrefab;
    private WeaponInput weaponInput;

    [Header("Weapon Options")]
    public ModifiableFloat moveSpeed = new ModifiableFloat(10f);
    public ModifiableFloat aimingMult = new ModifiableFloat(0.5f, 0f, 1f);

    public ModifiableFloat attacksPerSecond = new ModifiableFloat(1f, 0.0001f, 60f);
    protected float FireRate => 1f / attacksPerSecond.Value;

    public DamageStats damageStats = new DamageStats();

    public ModifiableFloat lightningRange = new ModifiableFloat(5f, 0f, 20f);
    public ModifiableFloat lightningChains = new ModifiableFloat(1f, 0f, 20f);
    public ModifiableFloat acidDamage = new ModifiableFloat(5f, 0f, 20f);

    public ModifiableFloat igniteChance = new ModifiableFloat(0f, 0f, 1f);
    public ModifiableFloat armourPierce = new ModifiableFloat(0f, 0f, 10f);

    protected float _lastShot = 0f;

    public SpriteRenderer weaponRenderer;

    public GameObject attackPrefab;

    public Transform firingPoint;

    protected List<AbilityEffect> unlockedAbilities = new List<AbilityEffect>();

    protected int[] powers;

    public float Experience => _experience;
    protected float _experience = 0;
    public float ExperienceNeeded => _experienceNeeded;
    protected float _experienceNeeded = 100f;
    public int Level => _level;
    protected int _level = 0;
    public int UpgradePoints => _upgradePoints;
    protected int _upgradePoints = 0;

    protected bool leftIsClockwise = false;

    /*
    public delegate void EnemyHitEvent(Weapon origin, Enemy target);
    public EnemyHitEvent EnemyHit;

    public delegate void WeaponFiredEvent(Weapon origin);
    public WeaponFiredEvent WeaponFired;
    */

    void Start()
    {
        CreateInputObject();
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
        if (weaponInput.Firing)
        {
            _firing = true;
            if (Fire())
                _actualMoveSpeed = moveSpeed.Value * aimingMult.Value;
        }
        else
            _firing = false;

        if (weaponInput.MovementDown)
        {
            if (Angle.Inside(0f + StaticRefs.SwapAngle, 180f - StaticRefs.SwapAngle))
                leftIsClockwise = false;
            else if (Angle.Inside(180f + StaticRefs.SwapAngle, 360f - StaticRefs.SwapAngle))
                leftIsClockwise = true;
        }

        //Move
        float input = weaponInput.Movement.x;
        if (leftIsClockwise)
            input = -input;
        if (input > 0)
        {
            MoveAround(-_actualMoveSpeed * Time.fixedDeltaTime);
        }
        if (input < 0)
        {
            MoveAround(_actualMoveSpeed * Time.fixedDeltaTime);
        }

        WeaponFixedUpdate();
    }

    private void Update()
    {
        WeaponUpdate();
    }

    protected void CreateInputObject()
    {
        WeaponInput newInput = PlayerInput.Instantiate(inputPrefab, controlScheme: "Keyboard_" + playerIndex, pairWithDevice: Keyboard.current).GetComponent<WeaponInput>();
        newInput.transform.SetParent(transform);
        weaponInput = newInput;
    }

    protected abstract void Setup();

    protected abstract bool Fire();

    protected virtual void WeaponUpdate() { }

    protected virtual void WeaponFixedUpdate() { }

    public virtual void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        statName = statName.ToLower();
        if (statName.Contains(" power"))
        {
            string simplifiedName = statName.Replace(" power", "");
            DamageType damageType;
            if (Enum.TryParse(simplifiedName, out damageType))
            {
                damageStats.ModifyDamage(damageType, modifierName, operation, value);
                return;
            }
        }

        switch (statName)
        {
            case "movespeed":
                moveSpeed.AddModifier(modifierName, value, operation);
                return;
            case "aimingmult":
                aimingMult.AddModifier(modifierName, value, operation);
                return;
            case "firerate":
                attacksPerSecond.AddModifier(modifierName, value, operation);
                return;
            case "acidDamage":
                acidDamage.AddModifier(modifierName, value, operation);
                return;
            case "armourPierce":
                armourPierce.AddModifier(modifierName, value, operation);
                return;
            case "igniteChance":
                igniteChance.AddModifier(modifierName, value, operation);
                return;
            case "lightningRange":
                lightningRange.AddModifier(modifierName, value, operation);
                return;
        }

        Debug.Log("Couldn't find stat with name: " + statName);
    }

    public abstract void UnlockPower(string powerName, int level);

    public void DefaultHit(Enemy enemy)
    {
        NormalBasicDamage(enemy);
        NormalAntimatterDamage(enemy);
        NormalAcidDamage(enemy);
        NormalNanitesDamage(enemy);
        NormalRadiationDamage(enemy);
        NormalPhysicalDamage(enemy);
        NormalHeatDamage(enemy);
        NormalLightningDamage(enemy);

        if (enemy.health <= 0)
            KillEnemy(enemy);
        else
            enemy.lastHitBy = this;
    }

    protected void NormalBasicDamage(Enemy enemy)
    {
        //Basic Damage
        enemy.health -= damageStats.basic.Value;
    }

    protected void NormalPhysicalDamage(Enemy enemy)
    {
        if (damageStats.physical.Value == 0)
            return;

        //Physical Damage
        float physicalDamage = DamageAfterArmour(enemy.Armour, DamageType.physical);
        if (enemy.Frozen)
            physicalDamage *= 2f;
        physicalDamage *= enemy.ResistanceMult(DamageType.physical);
        enemy.health -= physicalDamage;
    }

    protected void NormalHeatDamage(Enemy enemy)
    {
        if (damageStats.heat.Value == 0 && igniteChance.Value == 0)
            return;

        //Heat Damage
        float heatDamage = DamageAfterArmour(enemy.Armour, DamageType.heat);
        enemy.temperature += heatDamage;

        if (igniteChance.Value > Random.value)
            enemy.OnFire = true;
    }

    protected void NormalLightningDamage(Enemy enemy)
    {
        if (damageStats.lightning.Value == 0)
            return;

        //Lightning
        float lightningDamage = DamageAfterArmour(enemy.Armour, DamageType.lightning);
    }

    protected void NormalRadiationDamage(Enemy enemy)
    {
        if (damageStats.radiation.Value == 0)
            return;

        float radiationDamage = DamageAfterArmour(enemy.Armour, DamageType.radiation);
        enemy.radiation += radiationDamage;
    }

    protected void NormalAcidDamage(Enemy enemy)
    {
        if (damageStats.acid.Value == 0)
            return;

        float acidDamage = DamageAfterArmour(enemy.Armour, DamageType.acid);
        enemy.acid += acidDamage;
    }

    protected void NormalNanitesDamage(Enemy enemy)
    {
        if (damageStats.nanites.Value == 0)
            return;

        float nanitesDamage = DamageAfterArmour(enemy.Armour, DamageType.nanites);
        enemy.nanites += nanitesDamage;
    }

    protected void NormalAntimatterDamage(Enemy enemy)
    {
        if (damageStats.antimatter.Value == 0)
            return;

        float antimatterDamage = DamageAfterArmour(enemy.Armour, DamageType.antimatter);
        enemy.antimatter += antimatterDamage;
    }

    protected float DamageAfterArmour(int armourLevel, DamageType type)
    {
        if (type == DamageType.basic)
            return damageStats.GetDamage(type);

        float damage = damageStats.GetDamage(type);
        //Reduce damage by 5% per armour level
        damage -= damage * Mathf.Clamp(armourLevel - armourPierce.Value, 0, 10) * 0.05f;
        return damage;
    }

    public void KillEnemy(Enemy enemy)
    {
        GainExperience(enemy.XPReward);
        enemy.Destroy();
    }

    public void GainExperience(float xp)
    {
        _experience += xp;
        if (_experience > _experienceNeeded)
        {
            _experience -= _experienceNeeded;
            _level += 1;
            _upgradePoints += 1;
            _experienceNeeded += _experienceNeeded * 0.1f;
        }
    }

    public bool UseUpgradePoint()
    {
        if (_upgradePoints > 0)
        {
            --_upgradePoints;
            return true;
        }
        return false;
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
    heat,       //positive temperature change. Enemy takes heat damage when high enough
    cold,       //negative temperature change. Enemy freezes when low enough
    lightning,  //splits some of the damage to other nearby enemies based on conductivity value
    radiation,  //add radiation to target, target takes slow damage over time, often completely resisted
    acid,       //add acid to target, target takes quick damage over time, value reduces each time
    nanites,    //add nanites to target, target takes damage over time that goes down when their health gets lower and does nothing when below 10%.
    antimatter  //add antimatter to target, target explodes dealing basic damage to self and nearby enemies when hit by physical, acid or nanites.
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

[System.Serializable]
public class DamageStats
{
    public ModifiableFloat basic = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat physical = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat heat = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat cold = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat radiation = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat acid = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat lightning = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat nanites = new ModifiableFloat(0, 0, float.PositiveInfinity);
    public ModifiableFloat antimatter = new ModifiableFloat(0, 0, float.PositiveInfinity);

    public float GetDamage(DamageType type)
    {
        if (type == DamageType.physical)
            return physical.Value;
        else if (type == DamageType.heat)
            return heat.Value;
        else if (type == DamageType.cold)
            return cold.Value;
        else if (type == DamageType.radiation)
            return radiation.Value;
        else if (type == DamageType.acid)
            return acid.Value;
        else if (type == DamageType.lightning)
            return lightning.Value;
        else if (type == DamageType.nanites)
            return nanites.Value;
        else if (type == DamageType.antimatter)
            return antimatter.Value;
        else
            return basic.Value;
    }

    public void ModifyDamage(DamageType type, string modifierName, StatChangeOperation operation, float value)
    {
        if (type == DamageType.physical)
            physical.AddModifier(modifierName, value, operation);
        else if (type == DamageType.heat)
            heat.AddModifier(modifierName, value, operation);
        else if (type == DamageType.cold)
            heat.AddModifier(modifierName, value, operation);
        else if (type == DamageType.radiation)
            radiation.AddModifier(modifierName, value, operation);
        else if (type == DamageType.acid)
            acid.AddModifier(modifierName, value, operation);
        else if (type == DamageType.lightning)
            lightning.AddModifier(modifierName, value, operation);
        else if (type == DamageType.nanites)
            nanites.AddModifier(modifierName, value, operation);
        else if (type == DamageType.antimatter)
            antimatter.AddModifier(modifierName, value, operation);
        else
            basic.AddModifier(modifierName, value, operation);
    }
}