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
        enemy.lastHitBy = explosion.triggerWeapon;
        enemy.ReduceHealthBy(enemy.Class == EnemyClass.tank ? explosion.totalAntimatter * 0.5f : explosion.totalAntimatter);
    }

    public class Physical
    {

        public static void DamageEnemy(float physicalDamage, Enemy enemy)
        {
            if (enemy.acid > 0)
                physicalDamage -= 1f;
            if (enemy.Frozen)
                physicalDamage *= ColdStats.violentShatter ? 3f : 2f;
            enemy.ReduceHealthBy(physicalDamage);
            enemy.triggerAntimatter = true;
        }
    }
    public static Physical PhysicalStats = new Physical();

    public class Heat
    {
        public int hotterFire = 0;

        public static void HeatEnemy(float heatDamage, Enemy enemy)
        {
            enemy.ChangeTemp(heatDamage);
        }

        public static void FireDamage(Enemy enemy)
        {
            enemy.temperature += 10f * (1 + HeatStats.hotterFire) * StaticRefs.TempTickRate;
        }
    }
    public static Heat HeatStats = new Heat();

    public class Cold
    {
        public int earlyFreeze = 0;
        public int refraction = 0;
        public bool temperedIce = false;
        public bool violentShatter = false;

        public static void ChillEnemy(float coldDamage, Enemy enemy)
        {
            enemy.ChangeTemp(-coldDamage);
            if (ColdStats.earlyFreeze > 0)
            {
                for (int i = 0; i < ColdStats.earlyFreeze; ++i)
                {
                    if (-Random.value > enemy.NormalisedTemp)
                        enemy.Frozen = true;
                }
            }
        }
    }
    public static Cold ColdStats = new Cold();

    public class Lightning
    {
        public static void StrikeEnemy(float lightningDamage, Enemy enemy)
        {
            if (enemy.acid > 0)
                lightningDamage *= Acid.LightningMultiplier();
            if (enemy.nanites > 0)
                lightningDamage += DamageEvents.Nanites.LightningBoost(enemy);
            enemy.ReduceHealthBy(lightningDamage);
        }
    }
    public static Lightning LightningStats = new Lightning();

    public class Radiation
    {

        public static void IrradiateEnemy(float radiationValue, Enemy enemy)
        {
            enemy.radiation += radiationValue;
        }
    }
    public static Radiation RadiationStats = new Radiation();

    public class Acid
    {
        public bool conductiveAcid = false;

        public static void SplashEnemy(float acidValue, float acidDPS, Enemy enemy)
        {
            enemy.acid += acidValue;
            enemy.SetAcidDPS(acidDPS);
            enemy.triggerAntimatter = true;
        }

        public static float LightningMultiplier()
        {
            if (AcidStats.conductiveAcid)
                return 4f;
            return 2f;
        }
    }
    public static Acid AcidStats = new Acid();

    public class Nanites
    {
        public int selfReplication = 0;

        public static void SwarmEnemy(float nanitesValue, Enemy enemy)
        {
            enemy.nanites += nanitesValue;
            enemy.triggerAntimatter = true;
        }

        public static void DamageTick(Enemy enemy)
        {
            if (NanitesStats.selfReplication > 0)
                enemy.nanites += 10 * NanitesStats.selfReplication;

            float damageCutoff = enemy.MaxHealth * StaticRefs.NanitesCutoff;
            if (enemy.Health <= damageCutoff)
                return;

            //Note: DOT effects are effected by resistances when applied, not when dealing damage
            enemy.DOTDamage(Mathf.Min(enemy.nanites * (enemy.Health / enemy.MaxHealth) * StaticRefs.NanitesTickRate, enemy.Health - damageCutoff));
        }

        public static float LightningBoost(Enemy enemy)
        {
            float bonusDamage = enemy.nanites;
            enemy.nanites = 0;
            StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
            return bonusDamage;
        }
    }
    public static Nanites NanitesStats = new Nanites();

    public class Antimatter
    {

        public static void CoatEnemy(float antimatterValue, Enemy enemy)
        {
            enemy.antimatter += antimatterValue;
        }
    }
    public static Antimatter AntimatterStats = new Antimatter();

    public static void ResetAllStats()
    {
        PhysicalStats = new Physical();
        HeatStats = new Heat();
        ColdStats = new Cold();
        LightningStats = new Lightning();
        RadiationStats = new Radiation();
        AcidStats = new Acid();
        NanitesStats = new Nanites();
        AntimatterStats = new Antimatter();
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
