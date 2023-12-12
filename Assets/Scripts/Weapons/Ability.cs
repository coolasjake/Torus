using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
public class Ability : ScriptableObject
{
    public int level = 1;
    public int maxLevel = 20;
    [TextArea(3, 8)]
    public string description;
    public List<AbilityEffect> effects;
}

[System.Serializable]
public class AbilityEffect
{
    [Tooltip("Name of the stat or power being effected.")]
    public string name = "";
    [Tooltip("Larger numbers will be applied later, and override effects of lower numbers")]
    public int priority = 0;
    public bool isPower = false;
    public StatChangeOperation operation = StatChangeOperation.Multiply;
    public float change = 1f;

    public void Apply(Weapon weapon, string abilityName)
    {
        Debug.Log("Applying " + name);
        if (isPower)
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