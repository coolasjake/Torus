using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatSettings", menuName = "ScriptableObjects/CombatSettings", order = 1)]
public class CombatSettings : ScriptableObject
{
    
    public Vector2 torusScale = Vector2.one;

    public GameObject inputPrefab;

    public LayerMask attackMask;

    public GameObject stationExplosionPrefab;

    [Header("UI and Effects")]

    
    public InspectorClass_SpriteRefs UIRefs = new InspectorClass_SpriteRefs();
    [System.Serializable]
    public class InspectorClass_SpriteRefs
    {
        public HealthBar healthBarPrefab;

        public List<Sprite> healthArmourBorders = new List<Sprite>();
        [EnumNamedArray(typeof(DamageType))]
        public Sprite[] damageTypeIcons = new Sprite[System.Enum.GetNames(typeof(DamageType)).Length];
        public List<Sprite> upgradeLvlIcons = new List<Sprite>();
    }

    
    public InspectorClass_EffectSettings effectSettings = new InspectorClass_EffectSettings();
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
        public NuclearExplosion nuclearExplosionPrefab;
    }

    [Header("Enemy Settings")]
    
    [Min(0)]
    [Tooltip("How far an enemy will move per update with a speed of 1.")]
    public float baseEnemySpeed = 0.1f;

    
    [Min(0)]
    [Tooltip("How far a dodge enemy will move when it dodges.")]
    public float dodgeDist = 0.5f;

    /// <summary> The height that fast enemies start boosting (using their ability to move faster) at. </summary>
    
    public float boostStartingHeight = 6f;

    [Header("Controls Settings")]
    [Min(-90f)]
    [Tooltip("Controls the angle that a weapon has to have before the left/right movement direction is flipped to be more intuitive.")]
    public float weaponDirectionSwapBuffer = 0f;

    [Header("Damage Type Settings")]
    public InspectorClass_DamageSettings damageSettings = new InspectorClass_DamageSettings();

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
        [Tooltip("Default time that fire lasts after an enemy is ignited.")]
        public float fireDuration = 3f;
        [Min(0)]
        [Tooltip("Controls how often radiation deals damage, and how long before the first tick starts (unlike other DOTs).")]
        public float timeBetweenRadiationTicks = 2f;
        [Min(0)]
        [Tooltip("Value of radiation at which double damage and other effects trigger.")]
        public float criticalMassThreshold = 100f;
        [Min(0)]
        [Tooltip("Minimum time between lightning hits from the same weapon.")]
        public float lightningGroundedDur = 0.5f;
    }
}
