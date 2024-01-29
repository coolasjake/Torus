using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Railgun : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.Railgun;
    }

    [Header("Railgun Stats")]
    public ModifiableFloat bulletSpeed = new ModifiableFloat(10f, 0.01f, 1000f);
    public ModifiableFloat pierces = new ModifiableFloat(1f, 0f, 100f);
    [Min(1)]
    public ModifiableFloat shotsPerCharge = new ModifiableFloat(1f, 1f, 10f);
    private int _shotsSinceCharge = 0;
    public float minTimeBetweenShots = 0.2f;
    public ModifiableFloat shockwaveRadius = new ModifiableFloat(1f, 0.01f, 5f);
    public ModifiableFloat shockwaveDamageMult = new ModifiableFloat(0.01f, 0.01f, 1f);
    private float hotrodPhysToHeatAmount = 0.5f;
    public float haltingTime = 0.2f;

    [Header("Railgun Refs")]
    public LineRenderer aimingLaser;
    public GameObject shockwavePrefab;

    private RaycastHit2D aimLaserHit;

    private RailRod rodPrefab;

    protected override bool Fire()
    {
        if (_shotsSinceCharge >= shotsPerCharge.Value)
        {
            if (Time.time > _lastShot + FireRate)
                _shotsSinceCharge = 0;
            else
                return true;
        }

        if (Time.time > _lastShot + minTimeBetweenShots)
        {
            RailRod newRod = Instantiate(rodPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newRod.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newRod.railgun = this;
            newRod.remainingPierces = Mathf.RoundToInt(pierces.Value);
            _lastShot = Time.time;
            _shotsSinceCharge += 1;
        }
        return (Time.time < _lastShot + haltingTime);
    }

    protected override void WeaponUpdate()
    {
        if (powers[(int)RailGunPowers.AimLaser] > 0)
        {
            aimLaserHit = Physics2D.Raycast(aimingLaser.transform.position, aimingLaser.transform.up, 5f, StaticRefs.AttackMask);
            if (aimLaserHit.collider != null)
                aimingLaser.SetPosition(1, aimLaserHit.point);
        }
    }

    protected override void WeaponFixedUpdate()
    {
        if (powers[(int)RailGunPowers.AimLaser] > 1)
        {
            if (aimLaserHit.collider != null)
            {
                Enemy enemy = aimLaserHit.rigidbody.GetComponent<Enemy>();
                enemy.radiation += 10;
                enemy.lastHitBy = this;
            }
        }
    }

    public void RodHit(RailRod rod, Enemy enemy)
    {
        if (enemy.CheckDodge(rod.transform.position))
            return;
        StaticRefs.SpawnExplosion(0.5f, rod.transform.position);
        DefaultHit(enemy);

        //Shockwave if this is the first hit
        if (powers[(int)RailGunPowers.Shockwave] > 0 && rod.remainingPierces == Mathf.RoundToInt(pierces.Value) && damageStats.physical.Value > 0)
        {
            if (shockwavePrefab != null)
            {
                float scale = shockwaveRadius.Value;
                GameObject explosion = Instantiate(shockwavePrefab, rod.transform.position, Quaternion.identity);
                explosion.transform.localScale = new Vector3(scale, scale, scale);
            }
            Collider2D[] colliders = Physics2D.OverlapCircleAll(rod.transform.position, shockwaveRadius.Value);
            foreach (Collider2D collider in colliders)
            {
                enemy = collider.GetComponent<Enemy>();
                if (enemy)
                    ShockwaveHit(enemy, rod.transform.position);
            }
        }

        if (rod.remainingPierces-- <= 0)
            Destroy(rod.gameObject);
    }

    private void ShockwaveHit(Enemy enemy, Vector2 rodPos)
    {
        if (enemy.CheckDodge(rodPos))
            return;
        //Physical Damage
        float physicalDamage = DamageAfterArmour(enemy, DamageType.physical) * shockwaveDamageMult.Value;
        enemy.lastHitBy = this;
        DamageEvents.Physical.DamageEnemy(physicalDamage, enemy);
    }

    protected void HotrodHeatHit(Enemy enemy)
    {
        if ((damageStats.heat.Value == 0 || damageStats.physical.Value == 0) && igniteChance.Value == 0)
            return;

        //Heat Damage
        float heatDamage = damageStats.GetDamage(DamageType.physical) * hotrodPhysToHeatAmount * DamageEvents.ArmourMult(DamageType.heat, enemy, Mathf.RoundToInt(armourPierce.Value));
        enemy.lastHitBy = this;
        DamageEvents.Heat.HeatEnemy(heatDamage, enemy);

        if (Random.value < igniteChance.Value)
        {
            DamageEvents.Heat.IgniteEnemy(fireDurationMult.Value, enemy);
        }
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "bullet speed":
                bulletSpeed.AddModifier(modifierName, value, operation);
                return;
            case "pierces":
                pierces.AddModifier(modifierName, value, operation);
                return;
            case "charges":
                shotsPerCharge.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        RailGunPowers power;
        if (Enum.TryParse(powerName, out power))
        {
            powers[(int)power] = level;
            Debug.Log("Upgrading power: " + powerName + " to " + level);
        }
        else
            Debug.Log("Couldn't find power with name: " + powerName);

        switch (power)
        {
            case RailGunPowers.AimLaser:
                if (level == 1)
                    aimingLaser.gameObject.SetActive(true);
                break;
            case RailGunPowers.HotRods:
                HeatHit = HotrodHeatHit;
                break;
        }
    }

    protected override void Setup()
    {
        rodPrefab = attackPrefab.GetComponent<RailRod>();
        powers = new int[Enum.GetNames(typeof(RailGunPowers)).Length];
    }

    private enum RailGunPowers
    {
        AimLaser,
        HardeningRadiation,
        Shockwave,
        HotRods,
    }
}
