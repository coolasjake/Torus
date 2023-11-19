using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUI : MonoBehaviour
{
    private AbilityGroup chosenGroup;
    private int abilityIndex = 0;
}

public class ValidAbilityGroups : ScriptableObject
{
    public List<AbilityGroup> validAbilityGroups;
}

public class AbilityGroup : ScriptableObject
{
    public WeaponType targetType;
    public List<string> preReqAbilities = new List<string>();
    public List<string> incompatibleAbilities = new List<string>();
    public List<Ability> abilities = new List<Ability>();
}

public class Ability : ScriptableObject
{
    //public string name;
    public string description;
    public List<AbilityEffect> effects;
}

public abstract class AbilityEffect
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