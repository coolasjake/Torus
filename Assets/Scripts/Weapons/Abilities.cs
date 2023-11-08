using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public WeaponType targetType;
    [Tooltip("Larger numbers will be applied later, and override effects of lower numbers")]
    public int priority = 0;

    public abstract void Apply(Weapon weapon);
}

public class AttackPrefabChange : Ability
{
    public GameObject newAttackPrefab;

    public override void Apply(Weapon weapon)
    {
        weapon.attackPrefab = newAttackPrefab;
    }
}
public class AppearanceChange : Ability
{
    public Sprite newWeaponSprite; //May need to change to account for animations etc

    public override void Apply(Weapon weapon)
    {
        weapon.weaponRenderer.sprite = newWeaponSprite;
    }
}

public class StatChange : Ability
{
    public string statName;
    public StatChangeOperation operation = StatChangeOperation.Add;
    public float newValue;

    public override void Apply(Weapon weapon)
    {
        weapon.ChangeStat(statName, operation, newValue);
    }

}
public enum StatChangeOperation
{
    Add,
    Multiply,
    Set
}