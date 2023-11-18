using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticRefs : MonoBehaviour
{
    private static StaticRefs singleton;

    [SerializeField]
    private Canvas healthBarCanvas;

    [SerializeField]
    private List<Sprite> healthArmourBorders = new List<Sprite>();

    [SerializeField]
    private HealthBar healthBarPrefab;

    public static Sprite ArmourBorder(int level)
    {
        level = Mathf.Clamp(level, 0, singleton.healthArmourBorders.Count - 1);
        return singleton.healthArmourBorders[level];
    }

    public static HealthBar SpawnHealthBar(int armourLvl)
    {
        HealthBar HB = Instantiate(singleton.healthBarPrefab, singleton.healthBarCanvas.transform);
        HB.SetArmour(armourLvl);
        return HB;
    }

    void Awake()
    {
        singleton = this;
    }
}
