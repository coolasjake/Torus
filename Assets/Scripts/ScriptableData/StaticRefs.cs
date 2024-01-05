using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private Vector2 torusScale = Vector2.one;

    [SerializeField]
    private GameObject inputPrefab;

    public static WeaponInput SpawnInputPrefab(Transform parent, int index, string scheme)
    {
        WeaponInput newInput = PlayerInput.Instantiate(singleton.inputPrefab, playerIndex: index, controlScheme: scheme, pairWithDevice: Keyboard.current).GetComponent<WeaponInput>();
        newInput.transform.SetParent(parent);
        return newInput;
    }

    [SerializeField]
    private LayerMask attackMask;
    public static LayerMask AttackMask => singleton.attackMask;

    [SerializeField]
    private GameObject stationExplosionPrefab;

    public static void SpawnStationExplosion(float maxSize)
    {
        if (singleton.stationExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(singleton.stationExplosionPrefab, TorusMotion.torusOrigin, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
        }
    }

    [Header("UI and Effects")]

    [SerializeField]
    private InspectorClass_SpriteRefs UIRefs = new InspectorClass_SpriteRefs();
    [System.Serializable]
    public class InspectorClass_SpriteRefs
    {
        public Canvas healthBarCanvas;

        public HealthBar healthBarPrefab;

        public List<Sprite> healthArmourBorders = new List<Sprite>();
        [EnumNamedArray(typeof(DamageType))]
        public Sprite[] damageTypeIcons = new Sprite[System.Enum.GetNames(typeof(DamageType)).Length];
        public List<Sprite> upgradeLvlIcons = new List<Sprite>();
    }

    [SerializeField]
    private InspectorClass_EffectSettings effectSettings = new InspectorClass_EffectSettings();
    [System.Serializable]
    public class InspectorClass_EffectSettings
    {
        public GameObject frozenEffectPrefab;
        public GameObject fireEffectPrefab;
        public GameObject nanitesEffectPrefab;
        public GameObject acidEffectPrefab;
        public GameObject antimatterEffectPrefab;
        public GameObject tempEffectPrefab;
        public Color coldColour = Color.blue;
        public Color hotColour = Color.red;
        public GameObject explosionPrefab;
        public GameObject acidExplosionPrefab;
        public LightningObj lightningPrefab;
        public GameObject lightningExplosionPrefab;
        public AntimatterExplosion antimatterExplosionPrefab;
    }

    public static void SpawnExplosion(float scale, Vector2 pos)
    {
        if (singleton.effectSettings.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(singleton.effectSettings.explosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnAcidExplosion(float scale, Vector2 pos)
    {
        if (singleton.effectSettings.acidExplosionPrefab != null)
        {
            //scale = scale * 0.5f;
            //Debug.LogWarning("Acid explosion is scaled down.");
            GameObject explosion = Instantiate(singleton.effectSettings.acidExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static void SpawnLightningExplosion(float scale, Vector2 pos)
    {
        if (singleton.effectSettings.lightningExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(singleton.effectSettings.lightningExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public static AntimatterExplosion SpawnAntimatterExplosion(Vector2 pos, Enemy enemy)
    {
        if (singleton.effectSettings.antimatterExplosionPrefab != null)
        {
            AntimatterExplosion explosion = Instantiate(singleton.effectSettings.antimatterExplosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.CollectAntimatterFrom(enemy);
            explosion.transform.localScale = Vector3.zero;
            explosion.triggerWeapon = enemy.lastHitBy;
            return explosion;
        }
        return null;
    }

    public static Sprite ArmourBorder(int level)
    {
        level = Mathf.Clamp(level, 0, singleton.UIRefs.healthArmourBorders.Count - 1);
        return singleton.UIRefs.healthArmourBorders[level];
    }

    public static Sprite UpgradeLvlIcon(int level)
    {
        level = Mathf.Clamp(level, 0, singleton.UIRefs.upgradeLvlIcons.Count - 1);
        return singleton.UIRefs.upgradeLvlIcons[level];
    }

    public static Sprite DamageTypeIcon(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, singleton.UIRefs.damageTypeIcons.Length - 1);
        return singleton.UIRefs.damageTypeIcons[typeAsInt];
    }

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        HealthBar HB = Instantiate(singleton.UIRefs.healthBarPrefab, singleton.UIRefs.healthBarCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    public static GameObject SpawnFrozenEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.frozenEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnFireEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.fireEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnNanitesEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.nanitesEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAcidEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.acidEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static GameObject SpawnAntimatterEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.antimatterEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.Size, enemy.Size, enemy.Size);
        return effect;
    }

    public static SpriteRenderer SpawnTempEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.tempEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.GetComponent<SpriteMask>().sprite = enemy.spriteRenderer.sprite;
        return effect.GetComponent<SpriteRenderer>();
    }

    public static void SpawnLightning(Enemy enemyA, Enemy enemyB)
    {
        LightningObj effect = Instantiate(singleton.effectSettings.lightningPrefab, singleton.transform);
        effect.SetTargets(enemyA, enemyB);
    }

    public static Color ColdColour(float coldValue)
    {
        return singleton.effectSettings.coldColour.WithAlpha(Mathf.Clamp01(coldValue) * 0.5f);
    }

    public static Color HotColour(float hotValue)
    {
        return singleton.effectSettings.hotColour.WithAlpha(Mathf.Clamp01(hotValue) * 0.5f);
    }

    [Header("Enemy Settings")]
    [SerializeField]
    [Min(0)]
    [Tooltip("How far an enemy will move per update with a speed of 1.")]
    private float baseEnemySpeed = 0.1f;

    [SerializeField]
    [Min(0)]
    [Tooltip("How far a dodge enemy will move when it dodges.")]
    private float dodgeDist = 0.5f;
    public static float BaseSpeed => singleton.baseEnemySpeed;
    public static float DodgeDist => singleton.dodgeDist;

    /// <summary> The height that fast enemies start boosting (using their ability to move faster) at. </summary>
    [SerializeField]
    private float boostStartingHeight = 6f;
    public static float BoostStartingHeight => singleton.boostStartingHeight;

    [Header("Controls Settings")]
    [SerializeField]
    [Min(-90f)]
    [Tooltip("Controls the angle that a weapon has to have before the left/right movement direction is flipped to be more intuitive.")]
    private float weaponDirectionSwapBuffer = 0f;

    public static float SwapAngle => singleton.weaponDirectionSwapBuffer;

    [Header("Damage Type Settings")]
    [SerializeField]
    private InspectorClass_DamageSettings damageSettings = new InspectorClass_DamageSettings();

    [System.Serializable]
    public class InspectorClass_DamageSettings
    {
        [EnumNamedArray(typeof(DamageType))]
        public DamageTypeFlags[] typeCompatibilities = new DamageTypeFlags[System.Enum.GetNames(typeof(DamageType)).Length];

        [EnumNamedArray(typeof(DamageType))]
        public Ability[] defaultAbilitiesForTypes = new Ability[System.Enum.GetNames(typeof(DamageType)).Length];

        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float timeBetweenAcidTicks = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often nanites deal damage.")]
        public float timeBetweenNaniteTicks = 0.75f;
        [Min(0)]
        [Tooltip("Controls the fraction of health where nanites cannot deal any damage.")]
        public float nanitesHealthCutoff = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often temperature deals damage and falls back towards resting.")]
        public float timeBetweenTempTicks = 0.5f;
        [Min(0)]
        [Tooltip("Controls how often radiation deals damage, and how long before the first tick starts (unlike other DOTs).")]
        public float timeBetweenRadiationTicks = 2f;
        [Min(0)]
        [Tooltip("Value of radiation at which double damage and other effects trigger.")]
        public float criticalMassThreshold = 100f;
    }

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static bool DamageTypesAreCompatible(DamageType typeA, DamageType typeB)
    {
        return singleton.damageSettings.typeCompatibilities[(int)typeB].Includes(typeA);
    }

    /// <summary> Check if type A is allowed with type B (order will matter if type compatabilities are not symetrical). </summary>
    public static Ability DefaultAbility(DamageType damageType)
    {
        int typeAsInt = Mathf.Clamp((int)damageType, 0, singleton.damageSettings.defaultAbilitiesForTypes.Length - 1);
        return singleton.damageSettings.defaultAbilitiesForTypes[typeAsInt];
    }

    public static bool DoAcidTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.damageSettings.timeBetweenAcidTicks;
    }
    public static float AcidTickDamage(float DPS)
    {
        return DPS * singleton.damageSettings.timeBetweenAcidTicks;
    }
    public static bool DoNanitesTick(float lastTick, bool frozen)
    {
        return Time.time >= lastTick + (singleton.damageSettings.timeBetweenNaniteTicks * (frozen ? 2f : 1f));
    }
    public static float NanitesCutoff => singleton.damageSettings.nanitesHealthCutoff;
    public static float NanitesTickRate => singleton.damageSettings.timeBetweenNaniteTicks;
    public static bool DoTempTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.damageSettings.timeBetweenTempTicks;
    }
    public static float TempTickRate => singleton.damageSettings.timeBetweenTempTicks;
    public static bool DoRadiationTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.damageSettings.timeBetweenRadiationTicks;
    }

    public static int debugCounter = 0;

    void Awake()
    {
        singleton = this;
        TorusMotion.torusScale = torusScale;
    }

    void OnDrawGizmos()
    {
        TorusMotion.torusScale = torusScale;
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