using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private StaticSettings dataObject;
    private static StaticSettings data;
    [SerializeField]
    private Canvas defaultCanvas;

    private const string path = "Data/";

    public static LayerMask AttackMask => data.attackMask;

    public static WeaponInput SpawnInputPrefab(Transform parent, int index, string scheme)
    {
        WeaponInput newInput = PlayerInput.Instantiate(data.inputPrefab, playerIndex: index, controlScheme: scheme, pairWithDevice: Keyboard.current).GetComponent<WeaponInput>();
        newInput.transform.SetParent(parent);
        return newInput;
    }

    public static WeaponInput SpawnHubInputPrefab(Transform parent, int index, string scheme)
    {
        WeaponInput newInput = PlayerInput.Instantiate(data.inputPrefab, playerIndex: index, controlScheme: scheme, pairWithDevice: Keyboard.current).GetComponent<WeaponInput>();
        newInput.transform.SetParent(parent);
        return newInput;
    }

    public static void SpawnStationExplosion(float maxSize)
    {
        if (data.stationExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(data.stationExplosionPrefab, TorusMotion.torusOrigin, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
        }
    }

    public static void SpawnExplosion(float scale, Vector2 pos)
    {
        if (data.effectSettings.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(data.effectSettings.explosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnAcidExplosion(float scale, Vector2 pos)
    {
        if (data.effectSettings.acidExplosionPrefab != null)
        {
            //scale = scale * 0.5f;
            //Debug.LogWarning("Acid explosion is scaled down.");
            GameObject explosion = Instantiate(data.effectSettings.acidExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnLightningExplosion(float scale, Vector2 pos)
    {
        if (data.effectSettings.lightningExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(data.effectSettings.lightningExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static AntimatterExplosion SpawnAntimatterExplosion(Vector2 pos, Enemy enemy)
    {
        if (data.effectSettings.antimatterExplosionPrefab != null)
        {
            AntimatterExplosion explosion = Instantiate(data.effectSettings.antimatterExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = Vector3.zero;
            explosion.triggerWeapon = enemy.lastHitBy;
            return explosion;
        }
        return null;
    }

    public static void SpawnNuclearExplosion(Vector2 pos)
    {
        if (data.effectSettings.lightningExplosionPrefab != null)
        {
            NuclearExplosion explosion = Instantiate(data.effectSettings.nuclearExplosionPrefab, pos, Quaternion.identity, singleton.transform);
        }
    }

    public static Sprite ArmourBorder(int level)
    {
        level = Mathf.Clamp(level, 0, data.UIRefs.healthArmourBorders.Count - 1);
        return data.UIRefs.healthArmourBorders[level];
    }

    public static Sprite UpgradeLvlIcon(int level)
    {
        level = Mathf.Clamp(level, 0, data.UIRefs.upgradeLvlIcons.Count - 1);
        return data.UIRefs.upgradeLvlIcons[level];
    }

    public static Sprite DamageTypeIcon(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, data.UIRefs.damageTypeIcons.Length - 1);
        return data.UIRefs.damageTypeIcons[typeAsInt];
    }

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        if (singleton.defaultCanvas == null)
            singleton.defaultCanvas = FindObjectOfType<Canvas>();

        HealthBar HB = Instantiate(data.UIRefs.healthBarPrefab, singleton.defaultCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    public static GameObject SpawnFrozenEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.frozenEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnFireEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.fireEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnNanitesEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.nanitesEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAcidEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.acidEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAntimatterEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.antimatterEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static SpriteRenderer SpawnTempEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(data.effectSettings.tempEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.GetComponent<SpriteMask>().sprite = enemy.spriteRenderer.sprite;
        return effect.GetComponent<SpriteRenderer>();
    }

    public static void SpawnLightning(Enemy enemyA, Enemy enemyB)
    {
        LightningObj effect = Instantiate(data.effectSettings.lightningPrefab, singleton.transform);
        effect.SetTargets(enemyA, enemyB);
    }

    public static Color ColdColour(float coldValue)
    {
        return data.effectSettings.coldColour.WithAlpha(Mathf.Clamp01(coldValue) * 0.5f);
    }

    public static Color HotColour(float hotValue)
    {
        return data.effectSettings.hotColour.WithAlpha(Mathf.Clamp01(hotValue) * 0.5f);
    }

    public static float BaseSpeed => data.baseEnemySpeed;
    public static float DodgeDist => data.dodgeDist;
    public static float BoostStartingHeight => data.boostStartingHeight;


    public static float SwapAngle => data.weaponDirectionSwapBuffer;

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static bool DamageTypesAreCompatible(DamageType typeA, DamageType typeB)
    {
        return data.damageSettings.typeCompatibilities[(int)typeB].Includes(typeA);
    }

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static Ability DefaultAbility(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, data.damageSettings.defaultAbilitiesForTypes.Length - 1);
        return data.damageSettings.defaultAbilitiesForTypes[typeAsInt];
    }

    public static bool DoAcidTick(float lastTick)
    {
        return Time.time >= lastTick + data.damageSettings.timeBetweenAcidTicks;
    }
    public static float AcidTickDamage(float DPS)
    {
        return DPS * data.damageSettings.timeBetweenAcidTicks;
    }
    public static bool DoNanitesTick(float lastTick, bool frozen)
    {
        return Time.time >= lastTick + (data.damageSettings.timeBetweenNaniteTicks * (frozen ? 2f : 1f));
    }
    public static float NanitesCutoff => data.damageSettings.nanitesHealthCutoff;
    public static float NanitesTickRate => data.damageSettings.timeBetweenNaniteTicks;
    public static bool DoTempTick(float lastTick)
    {
        return Time.time >= lastTick + data.damageSettings.timeBetweenTempTicks;
    }
    public static float FireDur => data.damageSettings.fireDuration;
    public static float TempTickRate => data.damageSettings.timeBetweenTempTicks;
    public static bool DoRadiationTick(float lastTick)
    {
        return Time.time >= lastTick + data.damageSettings.timeBetweenRadiationTicks;
    }
    public static float CriticalMass => data.damageSettings.criticalMassThreshold;
    public static float GroundedDur => data.damageSettings.lightningGroundedDur;

    public static int debugCounter = 0;

    void Awake()
    {
        singleton = this;
        data = dataObject;
        if (data == null)
            data = Resources.Load<StaticSettings>(path);
        TorusMotion.torusScale = data.torusScale;
    }

    void OnDrawGizmos()
    {
        TorusMotion.torusScale = data.torusScale;
    }
}

[System.Flags]
public enum WeaponType
{
    None = 0,
    MachineGun = 1 << 0,
    Railgun = 1 << 1,
    FlameThrower = 1 << 2,
    Laser = 1 << 3,
    MissileLauncher = 1 << 4,
    FreezeRay = 1 << 5,
    BoomerangChainsaw = 1 << 6,
    Antimatter = 1 << 7
}

public enum DamageType
{
    none = 0,
    basic = 1,      //default damage, uneffected by resistances or armor
    physical = 2,   //kinetic damage. Applied instantly, heavily effected by armor, deals bonus damage to frozen
    heat = 3,       //positive temperature change. Enemy takes heat damage when high enough
    cold = 4,       //negative temperature change. Enemy freezes when low enough
    lightning = 5,  //splits some of the damage to other nearby enemies based on conductivity value
    radiation = 6,  //add radiation to target, target takes slow damage over time, often completely resisted
    acid = 7,       //add acid to target, target takes quick damage over time, value reduces each time
    nanites = 8,    //add nanites to target, target takes damage over time that goes down when their health gets lower and does nothing when below 10%.
    antimatter = 9, //add antimatter to target, target explodes dealing basic damage to self and nearby enemies when hit by physical, acid or nanites.
}

[System.Flags]
public enum DamageTypeFlags
{
    none = 0,
    basic = 1 << 0,
    physical = 1 << 1,
    heat = 1 << 2,
    cold = 1 << 3,
    lightning = 1 << 4,
    radiation = 1 << 5,
    acid = 1 << 6,
    nanites = 1 << 7,
    antimatter = 1 << 8
}