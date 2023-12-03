using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private LayerMask attackMask;

    public static LayerMask AttackMask => singleton.attackMask;

    [Header("UI and Effects")]

    [SerializeField]
    private InspectorClass_SpriteRefs UIRefs = new InspectorClass_SpriteRefs();
    [System.Serializable]
    public class InspectorClass_SpriteRefs
    {
        public Canvas healthBarCanvas;

        public HealthBar healthBarPrefab;

        public List<Sprite> healthArmourBorders = new List<Sprite>();
        public List<Sprite> upgradeLvlIcons = new List<Sprite>();
    }

    [SerializeField]
    private InspectorClass_EffectSettings effectSettings = new InspectorClass_EffectSettings();
    [System.Serializable]
    public class InspectorClass_EffectSettings
    {
        public GameObject frozenEffectPrefab;
        public GameObject fireEffectPrefab;
        public GameObject tempEffectPrefab;
        public Color coldColour = Color.blue;
        public Color hotColour = Color.red;
        public GameObject explosionPrefab;
        public LightningObj lightningPrefab;
    }

    public static void SpawnExplosion(float scale, Vector2 pos)
    {
        if (singleton.effectSettings.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(singleton.effectSettings.explosionPrefab, pos, Quaternion.identity, singleton.transform);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
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

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        HealthBar HB = Instantiate(singleton.UIRefs.healthBarPrefab, singleton.UIRefs.healthBarCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    public static GameObject SpawnFrozenEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.frozenEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.EffectsScale, enemy.EffectsScale, enemy.EffectsScale);
        return effect;
    }

    public static GameObject SpawnFireEffect(Enemy enemy)
    {
        GameObject effect = Instantiate(singleton.effectSettings.fireEffectPrefab, enemy.transform.position, enemy.transform.rotation, enemy.transform);
        effect.transform.localScale = new Vector3(enemy.EffectsScale, enemy.EffectsScale, enemy.EffectsScale);
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
        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float timeBetweenAcidTicks = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float timeBetweenNaniteTicks = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float nanitesHealthCutoff = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float timeBetweenTempTicks = 0.2f;
        [Min(0)]
        [Tooltip("Controls how often acid deals damage.")]
        public float timeBetweenRadiationTicks = 0.2f;
    }

    public static bool DoAcidTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.damageSettings.timeBetweenAcidTicks;
    }
    public static float AcidTickDamage(float DPS)
    {
        return DPS * singleton.damageSettings.timeBetweenAcidTicks;
    }
    public static bool DoNanitesTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.damageSettings.timeBetweenNaniteTicks;
    }
    public static float NanitesCutoff => singleton.damageSettings.nanitesHealthCutoff;
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
    }
}
