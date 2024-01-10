using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
public class Ability : ScriptableObject
{
    public DamageType damageType = DamageType.none;
    public WeaponType allowedWeapons;
    public bool requireType = true;
    public int minWave = 1;
    public int maxWave = 20;
    [Min(0)]
    public int maxRepeats = 0;
    public AbilityType type = AbilityType.Normal;
    [TextArea(3, 8)]
    public string description;
    public List<AbilityEffect> effects;
    public List<string> preReqAbilities = new List<string>();
    public List<string> incompatibleAbilities = new List<string>();
}

[System.Serializable]
public class AbilityEffect
{
    [Tooltip("Name of the stat or power being effected. Special names include:" +
        "\n-[DamageType] Power -> change value of damage type." +
        "\n-Enable [DamageType] -> add type to upgrade options." +
        "\n-Disable [DamageType] -> remove type from upgrade options" +
        "\n(NOTE: damage value needs to be changed seperately, e.g. Cold Power -> Multiply by 0)" +
        "\n-Basic Stats: 'move speed', 'aiming mult', 'fire rate'.")]
    public string name = "";
    [Tooltip("Weapon Powers are represented by an int indexed to an Enum instead of a standalone variable, and must match the Enum exactly.")]
    public bool isWeaponPower = false;
    [Tooltip("Formula for modifiable floats is:\ndefaultValue * multiply * (percentage * 0.01f) + addition." +
        "\n-Multipliers are compounding (3x & 3x = 9x)\n-Percentages add together (300% & 300% = 600%)")]
    public StatChangeOperation operation = StatChangeOperation.Multiply;
    public float change = 1f;

    public void Apply(Weapon weapon, string abilityName)
    {
        if (isWeaponPower)
            weapon.UnlockPower(name, (int)change);
        else
            weapon.AddModifier(name, abilityName, operation, change);
    }
}

public enum StatChangeOperation
{
    Add,
    Multiply,
    Percentage,
    Set
}

public enum AbilityType
{
    Normal,
    Rare,
    ModeChange,
    TeamBoost,
}