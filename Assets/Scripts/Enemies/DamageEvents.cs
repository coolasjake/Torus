using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageEvents
{
    /// <summary> Reduce damage if this enemy is a tank or has some ability effecting it. </summary>
    public static float AfterAbilityReductions(this Enemy enemy, float damage)
    {
        if (enemy.Class == EnemyClass.tank)
            damage -= enemy.AbilityPower;

        if (enemy.Frozen && ColdStats.frozenFortress)
            damage *= 0.2f;

        return damage;
    }

    public static float ArmourMult(DamageType type, Enemy enemy, int pierce)
    {
        if (type == DamageType.basic)
            return 1f;

        int effectiveArmour = Mathf.Clamp(enemy.Armour - enemy.meltedArmour - enemy.dissolvedArmour - pierce, 0, 10);

        return 1f - effectiveArmour * (type == DamageType.physical ? 0.08f : 0.05f);
    }

    public static void BaseTempChange(Enemy enemy)
    {
        float tempChange = enemy.data.baseTempChange * StaticRefs.TempTickRate;
        if (NanitesStats.systemSabotage)
            tempChange *= 0.1f;
        enemy.temperature = Mathf.MoveTowards(enemy.temperature, enemy.data.restingTemp, tempChange);
    }

    public static float ApplySpecialSpeedModifiers(Enemy enemy, float currentModifier)
    {
        float newModifier = currentModifier;
        if (NanitesStats.systemSabotage)
            newModifier *= 0.5f;
        if (enemy.Frozen && ColdStats.frozenFortress)
            newModifier = 0;
        return newModifier;
    }

    public static void AcidFireExplosion(Enemy enemy)
    {
        float baseDamage = Mathf.Min(enemy.acid * 5f, 100f);
        if (AcidStats.explosiveAcid)
            baseDamage *= 2f;
        enemy.temperature += baseDamage * enemy.ResistanceMult(DamageType.heat);
        enemy.ReduceHealthBy(baseDamage * enemy.ResistanceMult(DamageType.physical));
        enemy.acid = 0;
        StaticRefs.SpawnAcidExplosion(enemy.Size, enemy.transform.position);
        if (HeatStats.exothermicReaction)
            Heat.IgniteEnemy(1f, enemy);
        else
            enemy.RemoveFire();
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
            enemy.ReduceHealthBy(physicalDamage * enemy.ResistanceMult(DamageType.physical));
            enemy.triggerAntimatter = true;
        }
    }
    public static Physical PhysicalStats = new Physical();

    public class Heat
    {
        public int hotterFire = 0;
        public bool exothermicReaction = false;

        public static void HeatEnemy(float heatDamage, Enemy enemy)
        {
            if (ColdStats.temperedIce && enemy.Frozen)
            {
                heatDamage *= enemy.ResistanceMult(DamageType.physical);
                enemy.ReduceHealthBy(heatDamage);
            }

            enemy.ChangeTemp(heatDamage * enemy.ResistanceMult(DamageType.heat));
        }

        public static void DamageTick(Enemy enemy)
        {
            if (enemy.temperature > enemy.data.maxSafeTemp)
            {
                float excess = enemy.temperature - enemy.data.maxSafeTemp;
                if (enemy.data.damageFromHot)
                    enemy.DOTDamage(excess * (NanitesStats.hotBots * 0.5f + 1f));
                //Reduce temp by a fraction of the excess
                //Note: should NOT be relative to tick rate, or damage will increase exponentially with rate instead of linearly
                enemy.temperature = Mathf.MoveTowards(enemy.temperature, enemy.data.restingTemp, excess * 0.5f);
            }
        }

        public static void IgniteEnemy(float multiplier, Enemy enemy)
        {
            enemy.SetOnFire(StaticRefs.FireDur * multiplier);
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
        public bool iceJacking = false;
        /// <summary> Freeze Ray ability: stop completely, but reduce damage by 80%. </summary>
        public bool frozenFortress = false;

        public static void ChillEnemy(float coldDamage, Enemy enemy)
        {
            enemy.ChangeTemp(-coldDamage * enemy.ResistanceMult(DamageType.cold));
            if (ColdStats.earlyFreeze > 0)
            {
                for (int i = 0; i < ColdStats.earlyFreeze; ++i)
                {
                    if (-Random.value > enemy.NormalisedTemp)
                        Cold.Freeze(enemy);
                }
            }
        }

        public static void DamageTick(Enemy enemy)
        {
            if (enemy.temperature < enemy.data.freezeTemp)
            {
                Cold.Freeze(enemy);
                if (enemy.data.damageFromCold)
                {
                    float excess = -(enemy.temperature - enemy.data.freezeTemp);
                    enemy.DOTDamage(excess);
                    //Reduce temp by a fraction of the excess
                    //Note: should NOT be relative to tick rate, or damage will increase exponentially with rate instead of linearly
                    enemy.temperature = Mathf.MoveTowards(enemy.temperature, enemy.data.restingTemp, excess * 0.5f);
                }
            }
        }

        public static void Freeze(Enemy enemy)
        {
            if (ColdStats.iceJacking)
                enemy.SetHealth(Mathf.Min(enemy.MaxHealth * 0.8f, enemy.Health));

            enemy.Frozen = true;
        }
    }
    public static Cold ColdStats = new Cold();

    public class Lightning
    {
        public bool conductiveNanites = false;

        public static void StrikeEnemy(float lightningDamage, Enemy enemy)
        {
            if (enemy.acid > 0)
                lightningDamage *= Acid.LightningMultiplier();
            if (enemy.nanites > 0)
                lightningDamage += DamageEvents.Nanites.LightningBoost(enemy);
            enemy.ReduceHealthBy(lightningDamage * enemy.ResistanceMult(DamageType.lightning));
        }
    }
    public static Lightning LightningStats = new Lightning();

    public class Radiation
    {
        public bool nuclearReaction = false;

        public static void IrradiateEnemy(float radiationValue, Enemy enemy)
        {
            enemy.radiation += radiationValue * enemy.ResistanceMult(DamageType.radiation);
        }

        public static void DamageTick(Enemy enemy)
        {
            float radiation = enemy.radiation;

            if (CheckCriticalMass(enemy))
            {
                if (RadiationStats.nuclearReaction)
                {
                    StaticRefs.SpawnNuclearExplosion(enemy.transform.position);
                    enemy.radiation = 0;
                    return;
                }
                radiation *= 2f;
            }
            
            enemy.DOTDamage(radiation);
        }

        public static bool CheckCriticalMass(Enemy enemy)
        {
            return enemy.radiation >= StaticRefs.CriticalMass * ((10f - NanitesStats.tinyPhysicists) / 10f);
        }
    }
    public static Radiation RadiationStats = new Radiation();

    public class Acid
    {
        public bool conductiveAcid = false;
        public bool explosiveAcid = false;

        public static void SplashEnemy(float acidValue, float acidDPS, Enemy enemy)
        {
            enemy.acid += acidValue * enemy.ResistanceMult(DamageType.acid);
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
        public bool heroicNanites = false;
        public int tinyPhysicists = 0;
        public int hotBots = 0;
        public bool systemSabotage = false;

        public static void SwarmEnemy(float nanitesValue, Enemy enemy)
        {
            enemy.nanites += nanitesValue * enemy.ResistanceMult(DamageType.radiation);
            enemy.triggerAntimatter = true;
        }

        public static void DamageTick(Enemy enemy)
        {
            if (NanitesStats.selfReplication > 0 && enemy.nanites < 50 * enemy.Size)
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
            if (NanitesStats.heroicNanites && enemy.nanites > 10)
                enemy.nanites = enemy.nanites / 2f;
            else
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
            enemy.antimatter += antimatterValue * enemy.ResistanceMult(DamageType.radiation);
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
