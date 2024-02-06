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

    public DamageTypeFlags incompatibleDamageTypes = DamageTypeFlags.none;
    public DamageTypeFlags existingDamageTypes = DamageTypeFlags.none;

    public int playerIndex = 0;
    public bool doDirectionSwapping = false;
    public WeaponInput Input => weaponInput;
    private WeaponInput weaponInput;

    [Header("Weapon Options")]
    public ModifiableFloat moveSpeed = new ModifiableFloat(90f);
    public ModifiableFloat aimingMult = new ModifiableFloat(0.5f, 0f, 1f);
    [Min(0f)]
    public float dampening = 0f;
    private float _velocity = 0;

    public ModifiableFloat attacksPerSecond = new ModifiableFloat(1f, 0.0001f, 60f);
    protected float FireRate => 1f / attacksPerSecond.Value;

    public DamageStats damageStats = new DamageStats();

    public ModifiableFloat lightningRange = new ModifiableFloat(5f, 0f, 20f);
    /// <summary> How many times lightning splits after the initial hit. </summary>
    public ModifiableFloat lightningChains = new ModifiableFloat(1f, 0f, 20f);
    /// <summary> How many splits lightning can do each time it hits an enemy. </summary>
    public ModifiableFloat lightningSplits = new ModifiableFloat(1f, 0f, 20f);
    public ModifiableFloat acidDamagePerSecond = new ModifiableFloat(5f, 0f, 20f);

    public ModifiableFloat igniteChance = new ModifiableFloat(0f, 0f, 1f);
    public ModifiableFloat fireDurationMult = new ModifiableFloat(3f, 0f, float.PositiveInfinity);
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
        Initialize();
    }

    public void Initialize()
    {
        if (weaponInput == null)
        {
            CreateInputObject();
            AssignDefaultEvents();
            Setup();
        }
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

        if (weaponInput.MovementDown && doDirectionSwapping)
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

        float dampeningForce = (1f / (dampening + 1f));
        if (input > 0)
            _velocity = Mathf.Lerp(_velocity, -_actualMoveSpeed, dampeningForce);
        else if (input < 0)
            _velocity = Mathf.Lerp(_velocity, _actualMoveSpeed, dampeningForce);
        else
            _velocity = Mathf.Lerp(_velocity, 0f, dampeningForce);
        Height = 1f;
        MoveAround(_velocity * Time.fixedDeltaTime);

        WeaponFixedUpdate();
    }

    private void Update()
    {
        if (PauseManager.Paused)
            return;

        WeaponUpdate();
    }

    protected void CreateInputObject()
    {
        weaponInput = StaticRefs.SpawnInputPrefab(transform, playerIndex, "Keyboard_" + playerIndex);
    }

    protected abstract void Setup();

    protected abstract bool Fire();

    protected virtual void WeaponUpdate() { }

    protected virtual void WeaponFixedUpdate() { }

    public void DefaultHit(Enemy enemy)
    {
        BasicHit?.Invoke(enemy);
        AntimatterHit?.Invoke(enemy);
        NanitesHit?.Invoke(enemy);
        AcidHit?.Invoke(enemy);
        RadiationHit?.Invoke(enemy);
        PhysicalHit?.Invoke(enemy);
        HeatHit?.Invoke(enemy);
        ColdHit?.Invoke(enemy);
        LightningHit?.Invoke(enemy);
    }

    protected EnemyHit BasicHit;
    protected EnemyHit PhysicalHit;
    protected EnemyHit HeatHit;
    protected EnemyHit ColdHit;
    protected EnemyHit LightningHit;
    protected EnemyHit RadiationHit;
    protected EnemyHit AcidHit;
    protected EnemyHit NanitesHit;
    protected EnemyHit AntimatterHit;

    protected void NormalBasicHit(Enemy enemy)
    {
        if (damageStats.basic.Value == 0)
            return;
        //Basic Damage
        enemy.lastHitBy = this;
        enemy.ReduceHealthBy(damageStats.basic.Value);
    }

    protected void NormalPhysicalHit(Enemy enemy)
    {
        if (damageStats.physical.Value == 0)
            return;

        //Physical Damage
        float physicalDamage = DamageAfterArmour(enemy, DamageType.physical);
        enemy.lastHitBy = this;
        DamageEvents.Physical.DamageEnemy(physicalDamage, enemy);
    }

    protected void NormalHeatHit(Enemy enemy)
    {
        if (damageStats.heat.Value == 0 && igniteChance.Value == 0)
            return;

        //Heat Damage
        float heatDamage = DamageAfterArmour(enemy, DamageType.heat);
        enemy.lastHitBy = this;
        DamageEvents.Heat.HeatEnemy(heatDamage, enemy);

        if (Random.value < igniteChance.Value)
        {
            DamageEvents.Heat.IgniteEnemy(fireDurationMult.Value, enemy);
        }
    }

    protected void NormalColdHit(Enemy enemy)
    {
        if (damageStats.cold.Value == 0)
            return;

        //Heat Damage
        float coldDamage = DamageAfterArmour(enemy, DamageType.cold);
        enemy.lastHitBy = this;
        DamageEvents.Cold.ChillEnemy(coldDamage, enemy);
    }

    protected void NormalLightningHit(Enemy enemy)
    {
        if (damageStats.lightning.Value == 0)
            return;

        if (enemy.LightningHit(this) == false)
            return;
        lightningDamageEvent?.Invoke(enemy);
        //Split to nearby enemies
        Enemy chainTarget = null;
        for (int i = 0; i < lightningSplits.Value; ++i)
        {
            chainTarget = ChooseLightningChain(enemy);
            if (chainTarget == null)
                break;
            int maxChains = Mathf.RoundToInt(lightningChains.Value);
            if (DamageEvents.LightningStats.conductiveNanites && chainTarget.nanites > 0)
                maxChains += 1;
            lightningDamageEvent?.Invoke(chainTarget);
            //Chain to enemies near split targets
            for (int j = 0; j < maxChains; ++j)
            {
                chainTarget = ChooseLightningChain(chainTarget);
                if (chainTarget == null)
                    break;
                if (DamageEvents.LightningStats.conductiveNanites && chainTarget.nanites > 0)
                    maxChains += 1;
                lightningDamageEvent?.Invoke(chainTarget);
            }
        }
        if (chainTarget == null)
            StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
    }

    protected Enemy ChooseLightningChain(Enemy startingEnemy)
    {
        if (startingEnemy.Frozen)
            return null;
        Enemy chosen = null;
        foreach (Enemy enemy in startingEnemy.nearbyEnemies)
        {
            if (enemy != null && Utility.WithinRange(enemy.transform.position, startingEnemy.transform.position, lightningRange.Value))
            {
                if (enemy.LightningHit(this))
                {
                    chosen = enemy;
                    StaticRefs.SpawnLightning(startingEnemy, chosen);
                    break;
                }
            }
        }
        return chosen;
    }

    protected void NormalLightningDamage(Enemy enemy)
    {
        float lightningDamage = DamageAfterArmour(enemy, DamageType.lightning);
        enemy.lastHitBy = this;
        DamageEvents.Lightning.StrikeEnemy(lightningDamage, enemy);
    }

    protected EnemyHit lightningDamageEvent;

    protected void NormalRadiationHit(Enemy enemy)
    {
        if (damageStats.radiation.Value == 0)
            return;

        float radiationDamage = DamageAfterArmour(enemy, DamageType.radiation);
        enemy.lastHitBy = this;
        DamageEvents.Radiation.IrradiateEnemy(radiationDamage, enemy);
    }

    protected void NormalAcidHit(Enemy enemy)
    {
        if (damageStats.acid.Value == 0)
            return;

        float acidStack = DamageAfterArmour(enemy, DamageType.acid);
        enemy.lastHitBy = this;
        DamageEvents.Acid.SplashEnemy(acidStack, acidDamagePerSecond.Value, enemy);
    }

    protected void NormalNanitesHit(Enemy enemy)
    {
        if (damageStats.nanites.Value == 0)
            return;

        float nanitesDamage = DamageAfterArmour(enemy, DamageType.nanites);
        enemy.lastHitBy = this;
        DamageEvents.Nanites.SwarmEnemy(nanitesDamage, enemy);
    }

    protected void NormalAntimatterHit(Enemy enemy)
    {
        if (damageStats.antimatter.Value == 0)
            return;

        float antimatterDamage = DamageAfterArmour(enemy, DamageType.antimatter);
        enemy.lastHitBy = this;
        DamageEvents.Antimatter.CoatEnemy(antimatterDamage, enemy);
    }

    protected float DamageAfterArmour(Enemy enemy, DamageType type)
    {
        return damageStats.GetDamage(type) * DamageEvents.ArmourMult(type, enemy, Mathf.RoundToInt(armourPierce.Value));
    }

    public void KillEnemy(Enemy enemy)
    {
        GainExperience(enemy.XPReward);
        enemy.Destroy();
    }

    public void GainExperience(float xp)
    {
        return;
        _experience += xp;
        if (_experience > _experienceNeeded)
        {
            _experience -= _experienceNeeded;
            _level += 1;
            _upgradePoints += 1;
            _experienceNeeded += _experienceNeeded * 0.1f;
        }
    }

    public void LevelUp()
    {
        _experience -= _experienceNeeded;
        _level += 1;
        _upgradePoints += 1;
        _experienceNeeded += _experienceNeeded * 0.1f;
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

    protected virtual void AssignDefaultEvents()
    {
        BasicHit = NormalBasicHit;
        PhysicalHit = NormalPhysicalHit;
        HeatHit = NormalHeatHit;
        ColdHit = NormalColdHit;
        LightningHit = NormalLightningHit;
        RadiationHit = NormalRadiationHit;
        AcidHit = NormalAcidHit;
        NanitesHit = NormalNanitesHit;
        AntimatterHit = NormalAntimatterHit;


        lightningDamageEvent += NormalLightningDamage;
    }

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

        if (statName.Contains("enable type "))
        {
            string simplifiedName = statName.Replace("enable type ", "");
            DamageType damageType;
            if (Enum.TryParse(simplifiedName, out damageType))
            {
                existingDamageTypes = existingDamageTypes.PlusType(damageType);
                return;
            }
        }

        if (statName.Contains("disable type "))
        {
            string simplifiedName = statName.Replace("disable type ", "");
            DamageType damageType;
            if (Enum.TryParse(simplifiedName, out damageType))
            {
                existingDamageTypes = existingDamageTypes.WithoutType(damageType);
                incompatibleDamageTypes = incompatibleDamageTypes.PlusType(damageType);
                return;
            }
        }

        switch (statName)
        {
            case "move speed":
                moveSpeed.AddModifier(modifierName, value, operation);
                return;
            case "aiming mult":
                aimingMult.AddModifier(modifierName, value, operation);
                return;
            case "fire rate":
                attacksPerSecond.AddModifier(modifierName, value, operation);
                return;
            case "armour pierce":
                armourPierce.AddModifier(modifierName, value, operation);
                return;
            //Heat upgrades
            case "ignite chance":
                igniteChance.AddModifier(modifierName, value, operation);
                return;
            case "fire duration":
                fireDurationMult.AddModifier(modifierName, value, operation);
                return;
            case "hotter fire":
                DamageEvents.HeatStats.hotterFire += 1;
                return;
            case "exothermic reaction":
                DamageEvents.HeatStats.exothermicReaction = true;
                return;
            //Lightning upgrades
            case "lightning range":
                lightningRange.AddModifier(modifierName, value, operation);
                return;
            case "lightning chains":
                lightningChains.AddModifier(modifierName, value, operation);
                return;
            case "lightning splits":
                lightningSplits.AddModifier(modifierName, value, operation);
                return;
            //Acid upgrades
            case "acid damage":
                acidDamagePerSecond.AddModifier(modifierName, value, operation);
                return;
            case "conductive acid":
                DamageEvents.AcidStats.conductiveAcid = true;
                return;
            //Cold upgrades
            case "early freeze":
                DamageEvents.ColdStats.earlyFreeze += 1;
                return;
            case "tempered ice":
                DamageEvents.ColdStats.temperedIce = true;
                return;
            case "crystal refraction":
                DamageEvents.ColdStats.refraction += 1;
                return;
            case "violent shattering":
                DamageEvents.ColdStats.violentShatter = true;
                return;
            case "ice jacking":
                DamageEvents.ColdStats.iceJacking = true;
                return;
            //Nanites upgrades
            case "self replication":
                DamageEvents.NanitesStats.selfReplication += 1;
                return;
            case "heroic nanites":
                DamageEvents.NanitesStats.heroicNanites = true;
                return;
            case "tiny physicists":
                DamageEvents.NanitesStats.tinyPhysicists += 1;
                return;
        }

        Debug.Log("Couldn't find stat with name: " + statName);
    }

    public abstract void UnlockPower(string powerName, int level);
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
        else if (type == DamageType.basic)
            return basic.Value;
        return 0;
    }

    public void ModifyDamage(DamageType type, string modifierName, StatChangeOperation operation, float value)
    {
        if (type == DamageType.physical)
            physical.AddModifier(modifierName, value, operation);
        else if (type == DamageType.heat)
            heat.AddModifier(modifierName, value, operation);
        else if (type == DamageType.cold)
            cold.AddModifier(modifierName, value, operation);
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
        else if (type == DamageType.basic)
            basic.AddModifier(modifierName, value, operation);
    }
}