using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private LayerMask attackMask;

    [SerializeField]
    private Canvas healthBarCanvas;

    [SerializeField]
    private List<Sprite> healthArmourBorders = new List<Sprite>();
    [SerializeField]
    private List<Sprite> upgradeLvlIcons = new List<Sprite>();

    [SerializeField]
    private HealthBar healthBarPrefab;

    [SerializeField]
    [Min(-90f)]
    [Tooltip("Controls the angle that a weapon has to have before the left/right movement direction is flipped to be more intuitive.")]
    private float weaponDirectionSwapBuffer = 0f;

    [SerializeField]
    [Min(0)]
    [Tooltip("Controls how often acid deals damage.")]
    private float timeBetweenAcidTicks = 0.2f;
    [SerializeField]
    [Min(0)]
    [Tooltip("Controls how often acid deals damage.")]
    private float timeBetweenNaniteTicks = 0.2f;
    [SerializeField]
    [Min(0)]
    [Tooltip("Controls how often acid deals damage.")]
    private float nanitesHealthCutoff = 0.2f;
    [SerializeField]
    [Min(0)]
    [Tooltip("Controls how often acid deals damage.")]
    private float timeBetweenTempTicks = 0.2f;
    [SerializeField]
    [Min(0)]
    [Tooltip("Controls how often acid deals damage.")]
    private float timeBetweenRadiationTicks = 0.2f;

    public static Sprite ArmourBorder(int level)
    {
        level = Mathf.Clamp(level, 0, singleton.healthArmourBorders.Count - 1);
        return singleton.healthArmourBorders[level];
    }

    public static Sprite UpgradeLvlIcon(int level)
    {
        level = Mathf.Clamp(level, 0, singleton.upgradeLvlIcons.Count - 1);
        return singleton.upgradeLvlIcons[level];
    }

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        HealthBar HB = Instantiate(singleton.healthBarPrefab, singleton.healthBarCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    public static LayerMask AttackMask => singleton.attackMask;

    public static float SwapAngle => singleton.weaponDirectionSwapBuffer;

    public static bool DoAcidTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.timeBetweenAcidTicks;
    }
    public static float AcidTickDamage(float DPS)
    {
        return DPS * singleton.timeBetweenAcidTicks;
    }
    public static bool DoNanitesTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.timeBetweenNaniteTicks;
    }
    public static float NanitesCutoff => singleton.nanitesHealthCutoff;
    public static bool DoTempTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.timeBetweenTempTicks;
    }
    public static bool DoRadiationTick(float lastTick)
    {
        return Time.time >= lastTick + singleton.timeBetweenRadiationTicks;
    }

    void Awake()
    {
        singleton = this;
    }
}
