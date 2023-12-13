using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageEvents
{
    public static EnemyHit AcidFireExplosion = DefaultAcidFireExplosion;
    public static void DefaultAcidFireExplosion(Enemy enemy)
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
    public static void DefaultAntimatterExplosion(Enemy enemy)
    {
        float size = Mathf.Log10(enemy.antimatter);
        //enemy.antimatter = 0; //Remove antimatter on explosion hit
        StaticRefs.SpawnAntimatterExplosion(size, enemy.transform.position);
    }

    public static float NanitesLightningBonus(Enemy enemy)
    {
        float bonusDamage = enemy.nanites;
        enemy.nanites = 0;
        StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
        return bonusDamage;
    }
}

public delegate void EnemyHit(Enemy enemy);
