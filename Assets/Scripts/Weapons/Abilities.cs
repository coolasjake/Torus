using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AbilityGroupData", menuName = "ScriptableObjects/AbilityGroupData", order = 1)]
public class AbilityGroupData : ScriptableObject
{
    public WeaponType targetType;
    public List<string> preReqAbilities = new List<string>();
    public List<string> incompatibleAbilities = new List<string>();
    public List<Ability> abilities = new List<Ability>();
}

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
public class Ability : ScriptableObject
{
    //public string name;
    public string description;
    public List<AbilityEffect> effects;
}

[System.Serializable]
public class AbilityEffect
{
    [Tooltip("Larger numbers will be applied later, and override effects of lower numbers")]
    public string name = "";
    public int priority = 0;
    public bool isPower = false;
    public string statName = "";
    public StatChangeOperation operation;
    public float change = 1f;

    public void Apply(Weapon weapon)
    {
        if (isPower)
            weapon.UnlockPower(statName, (int)change);
        else
            weapon.AddModifier(statName, name, operation, change);
    }
}

public enum StatChangeOperation
{
    Add,
    Multiply,
    Set
}