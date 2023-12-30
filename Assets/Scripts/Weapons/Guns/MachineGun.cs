using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MachineGun : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.MachineGun;
    }

    [Header("Machinegun Stats")]
    public ModifiableFloat randomSpreadAngle = new ModifiableFloat(10f, 0f, 90f);

    public ModifiableFloat bulletSpeed = new ModifiableFloat(10f, 0.01f, 1000f);

    //private float fireRateDebit = 0;

    private Bullet bulletPrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            Bullet newBullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            dir = dir.Rotate(Random.Range(-randomSpreadAngle.Value, randomSpreadAngle.Value));
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newBullet.machinegun = this;
            _lastShot = Time.time;
        }
        return true;
    }

    public void BulletHit(Bullet bullet, Enemy enemy)
    {
        if (enemy.CheckDodge(bullet.transform.position))
            return;
        DefaultHit(enemy);
        bullet.gameObject.SetActive(false);
        Destroy(bullet.gameObject);
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "spread":
                randomSpreadAngle.AddModifier(modifierName, value, operation);
                return;
            case "bullet speed":
                bulletSpeed.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        MachineGunPowers power;
        if (Enum.TryParse(powerName, out power))
        {
            powers[(int)power] = level;
        }
        else
            Debug.Log("Couldn't find power with name: " + powerName);
    }

    protected override void Setup()
    {
        bulletPrefab = attackPrefab.GetComponent<Bullet>();
        powers = new int[Enum.GetNames(typeof(MachineGunPowers)).Length];
        SetupDamageTypes();
    }

    private enum MachineGunPowers {
        DoubleShot,
        ExtraShot,
        SecondGun,
        ArmourPierce,
        EnemiesExplode,
        ArmourStrip,
        Fireballs,
        InstantFreeze,
    }
}