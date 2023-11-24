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

    [Header("Railgun Refs")]
    public LineRenderer aimingLaser;

    private RaycastHit2D aimLaserHit;

    private RailRod rodPrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            RailRod newBullet = Instantiate(rodPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newBullet.railgun = this;
            _lastShot = Time.time;
        }
        return true;
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
                enemy.radiation += 1f;
                enemy.lastHitBy = this;
            }
        }
    }

    public void RodHit(RailRod rod, Enemy enemy)
    {
        enemy.SpawnExplosion(0.5f, rod.transform.position);
        DefaultHit(enemy);
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "bulletSpeed":
                bulletSpeed.AddModifier(modifierName, value, operation);
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

        if (power == RailGunPowers.AimLaser)
        {
            if (level == 1)
                aimingLaser.gameObject.SetActive(true);
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
    }
}
