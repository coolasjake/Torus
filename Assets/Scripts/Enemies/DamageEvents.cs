using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageEvents
{
    public static void AcidFireExplosion(Enemy enemy)
    {
        enemy.temperature += Mathf.Min(enemy.acid * 5f, 100f);
        enemy.acid = 0;
        StaticRefs.SpawnAcidExplosion(enemy.Size, enemy.transform.position);
    }

    public static void IgniteWhileFrozen(Enemy enemy)
    {
        enemy.Frozen = false;
        enemy.RemoveFire();
        enemy.temperature = Mathf.Clamp(enemy.temperature, enemy.data.freezeTemp, enemy.data.maxSafeTemp);
    }
    public static void FreezeWhileOnFire(Enemy enemy)
    {
        enemy.RemoveFire();
        enemy.Frozen = false;
        enemy.temperature = Mathf.Clamp(enemy.temperature, enemy.data.freezeTemp, enemy.data.maxSafeTemp);
    }

    public static void CollectAntimatterFrom(this AntimatterExplosion explosion, Enemy enemy)
    {
        explosion.totalAntimatter += enemy.antimatter;
        enemy.antimatter = 0;
        enemy.nanites = 0;
        enemy.acid = 0;
    }

    public static void DealAntimatterDamageTo(this AntimatterExplosion explosion, Enemy enemy)
    {
        enemy.ReduceHealthBy(enemy.Class == EnemyClass.tank ? explosion.totalAntimatter * 0.5f : explosion.totalAntimatter, explosion.triggerWeapon);
    }

    public static float NanitesLightningBonus(Enemy enemy)
    {
        float bonusDamage = enemy.nanites;
        enemy.nanites = 0;
        StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
        return bonusDamage;
    }
    public static class Physical
    {

    }
    public static class Heat
    {

    }
    public static class Cold
    {

    }
    public static class Lightning
    {

    }
    public static class Radiation
    {

    }
    public static class Acid
    {

    }
    public static class Nanites
    {

    }
    public static class Antimatter
    {

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
