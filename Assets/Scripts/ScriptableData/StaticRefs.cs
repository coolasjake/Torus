using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private Canvas defaultCanvas;

    private const string path = "Data/";

    #region Combat Settings
    [SerializeField]
    private CombatSettings combatSettingsObject;
#if UNITY_EDITOR
    private static CombatSettings combatData { get => singleton.combatSettingsObject; set => singleton.combatSettingsObject = value; }
#else
    private static CombatSettings combatData;
#endif
    public static LayerMask AttackMask => combatData.attackMask;

    public static WeaponInput SpawnInputPrefab(Transform parent, int index, string scheme)
    {
        WeaponInput newInput = PlayerInput.Instantiate(combatData.inputPrefab, playerIndex: index, controlScheme: scheme, pairWithDevice: Keyboard.current).GetComponent<WeaponInput>();
        newInput.transform.SetParent(parent);
        return newInput;
    }

    public static void SpawnStationExplosion(float maxSize)
    {
        if (combatData.stationExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(combatData.stationExplosionPrefab, TorusMotion.torusOrigin, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
        }
    }

    public static void SpawnExplosion(float scale, Vector2 pos)
    {
        if (combatData.effectSettings.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(combatData.effectSettings.explosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnAcidExplosion(float scale, Vector2 pos)
    {
        if (combatData.effectSettings.acidExplosionPrefab != null)
        {
            //scale = scale * 0.5f;
            //Debug.LogWarning("Acid explosion is scaled down.");
            GameObject explosion = Instantiate(combatData.effectSettings.acidExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnLightningExplosion(float scale, Vector2 pos)
    {
        if (combatData.effectSettings.lightningExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(combatData.effectSettings.lightningExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static AntimatterExplosion SpawnAntimatterExplosion(Vector2 pos, Enemy enemy)
    {
        if (combatData.effectSettings.antimatterExplosionPrefab != null)
        {
            AntimatterExplosion explosion = Instantiate(combatData.effectSettings.antimatterExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = Vector3.zero;
            explosion.triggerWeapon = enemy.lastHitBy;
            return explosion;
        }
        return null;
    }

    public static void SpawnNuclearExplosion(Vector2 pos)
    {
        if (combatData.effectSettings.lightningExplosionPrefab != null)
        {
            NuclearExplosion explosion = Instantiate(combatData.effectSettings.nuclearExplosionPrefab, pos, Quaternion.identity, singleton.transform);
        }
    }

    public static Sprite ArmourBorder(int level)
    {
        level = Mathf.Clamp(level, 0, combatData.UIRefs.healthArmourBorders.Count - 1);
        return combatData.UIRefs.healthArmourBorders[level];
    }

    public static Sprite UpgradeLvlIcon(int level)
    {
        level = Mathf.Clamp(level, 0, combatData.UIRefs.upgradeLvlIcons.Count - 1);
        return combatData.UIRefs.upgradeLvlIcons[level];
    }

    public static Sprite DamageTypeIcon(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, combatData.UIRefs.damageTypeIcons.Length - 1);
        return combatData.UIRefs.damageTypeIcons[typeAsInt];
    }

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        if (singleton.defaultCanvas == null)
            singleton.defaultCanvas = FindObjectOfType<Canvas>();

        HealthBar HB = Instantiate(combatData.UIRefs.healthBarPrefab, singleton.defaultCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    public static GameObject SpawnFrozenEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.frozenEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnFireEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.fireEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnNanitesEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.nanitesEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAcidEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.acidEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAntimatterEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.antimatterEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static SpriteRenderer SpawnTempEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(combatData.effectSettings.tempEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.GetComponent<SpriteMask>().sprite = enemy.spriteRenderer.sprite;
        return effect.GetComponent<SpriteRenderer>();
    }

    public static void SpawnLightning(Enemy enemyA, Enemy enemyB)
    {
        LightningObj effect = Instantiate(combatData.effectSettings.lightningPrefab, singleton.transform);
        effect.SetTargets(enemyA, enemyB);
    }

    public static Color ColdColour(float coldValue)
    {
        return combatData.effectSettings.coldColour.WithAlpha(Mathf.Clamp01(coldValue) * 0.5f);
    }

    public static Color HotColour(float hotValue)
    {
        return combatData.effectSettings.hotColour.WithAlpha(Mathf.Clamp01(hotValue) * 0.5f);
    }

    public static float SpawningHeight => combatData.spawningHeight;

    public static float SpacingRate => combatData.spacingRate;
    public static float SpacingForce => combatData.spacingForce;
    public static float MaxSpacingMove => combatData.maxSpacingMove;
    public static float BaseSpeed => combatData.baseEnemySpeed;
    public static float DodgeDist => combatData.dodgeDist;
    public static float BoostStartingHeight => combatData.boostStartingHeight;


    public static float SwapAngle => combatData.weaponDirectionSwapBuffer;

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static bool DamageTypesAreCompatible(DamageType typeA, DamageType typeB)
    {
        return combatData.damageSettings.typeCompatibilities[(int)typeB].Includes(typeA);
    }

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static Ability DefaultAbility(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, combatData.damageSettings.defaultAbilitiesForTypes.Length - 1);
        return combatData.damageSettings.defaultAbilitiesForTypes[typeAsInt];
    }

    public static bool DoAcidTick(float lastTick)
    {
        return Time.time >= lastTick + combatData.damageSettings.timeBetweenAcidTicks;
    }
    public static float AcidTickDamage(float DPS)
    {
        return DPS * combatData.damageSettings.timeBetweenAcidTicks;
    }
    public static bool DoNanitesTick(float lastTick, bool frozen)
    {
        return Time.time >= lastTick + (combatData.damageSettings.timeBetweenNaniteTicks * (frozen ? 2f : 1f));
    }
    public static float NanitesCutoff => combatData.damageSettings.nanitesHealthCutoff;
    public static float NanitesTickRate => combatData.damageSettings.timeBetweenNaniteTicks;
    public static bool DoTempTick(float lastTick)
    {
        return Time.time >= lastTick + combatData.damageSettings.timeBetweenTempTicks;
    }
    public static float FireDur => combatData.damageSettings.fireDuration;
    public static float TempTickRate => combatData.damageSettings.timeBetweenTempTicks;
    public static bool DoRadiationTick(float lastTick)
    {
        return Time.time >= lastTick + combatData.damageSettings.timeBetweenRadiationTicks;
    }
    public static float CriticalMass => combatData.damageSettings.criticalMassThreshold;
    public static float GroundedDur => combatData.damageSettings.lightningGroundedDur;

    public static Weapon SpawnWeapon(WeaponType type, int playerIndex)
    {
        Vector2 pos = playerIndex == 0 ? Vector2.left : Vector2.right;
        Weapon weapon = Instantiate<Weapon>(combatData.weaponPrefabs[(int)type], pos, Quaternion.identity);
        weapon.playerIndex = playerIndex;
        return weapon;
    }
    #endregion

    #region Hub Settings
    [SerializeField]
    private HubSettings hubSettingsObject;
    private static HubSettings hubData;
    public static float TileSize => hubData.tileSize;

    public static HubInput SpawnHubInputPrefab(Transform parent, int index, string scheme)
    {
        HubInput newInput = PlayerInput.Instantiate(hubData.inputPrefab, playerIndex: index, controlScheme: scheme, pairWithDevice: Keyboard.current).GetComponent<HubInput>();
        newInput.transform.SetParent(parent);
        return newInput;
    }

    #endregion

    public static int debugCounter = 0;

    void Awake()
    {
        singleton = this;
        combatData = combatSettingsObject;
        if (combatData == null)
            combatData = Resources.Load<CombatSettings>(path);
        hubData = hubSettingsObject;
        if (hubData == null)
            hubData = Resources.Load<HubSettings>(path);
        TorusMotion.torusScale = combatData.torusScale;
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