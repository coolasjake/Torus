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

    [Header("Railgun Refs")]
    public LineRenderer aimingLaser;

    private RaycastHit2D aimLaserHit;

    private RailRod rodPrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            RailRod newRod = Instantiate(rodPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newRod.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newRod.railgun = this;
            newRod.remainingPierces = Mathf.RoundToInt(pierces.Value);
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
        if (rod.remainingPierces-- <= 0)
            Destroy(rod.gameObject);
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
