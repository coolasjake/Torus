using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageEvents
{
    public static EnemyHit AcidFireExplosion = DefaultAcidFireExplosion;
    private static void DefaultAcidFireExplosion(Enemy enemy)
    {
        enemy.temperature += Mathf.Min(enemy.acid * 5f, 100f);
        enemy.acid = 0;
        StaticRefs.SpawnAcidExplosion(enemy.Size, enemy.transform.position);
    }

    public static EnemyHit IceFire = DefaultIceFire;
    public static void DefaultIceFire(Enemy enemy)
    {
        enemy.Frozen = false;
        enemy.RemoveFire();
        enemy.temperature = Mathf.Clamp(enemy.temperature, enemy.data.freezeTemp, enemy.data.maxSafeTemp);
    }

    public static EnemyHit AntimatterExplosion = DefaultAntimatterExplosion;
    private static void DefaultAntimatterExplosion(Enemy enemy)
    {
        AntimatterExplosion aExplosion = StaticRefs.SpawnAntimatterExplosion(enemy.transform.position, enemy);
    }

    public static float NanitesLightningBonus(Enemy enemy)
    {
        float bonusDamage = enemy.nanites;
        enemy.nanites = 0;
        StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
        return bonusDamage;
    }

    public static bool Includes(this DamageTypeFlags flag, DamageType damageType)
    {
        DamageTypeFlags flagOfType = (DamageTypeFlags)(1 << (int)damageType - 1);
        if (flag.HasFlag(flagOfType))
            return true;
        return false;
    }

    public static DamageTypeFlags PlusType(this DamageTypeFlags flag, DamageType newType)
    {
        DamageTypeFlags flagOfType = (DamageTypeFlags)(1 << (int)newType - 1);
        return flag | flagOfType;
    }

    public static bool Includes(this WeaponType field, WeaponType value)
    {
        return (field & value) == value;
    }
}

public delegate void EnemyHit(Enemy enemy);
