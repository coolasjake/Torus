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
    public MG_Fireball fireballPrefab;
    public ModifiableFloat fireballSplash = new ModifiableFloat(0.5f, 0.01f, 3f);

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            if (powers[(int)MachineGunPowers.ExtraShot] > 0)
                ShootProjectile(0);

            if (powers[(int)MachineGunPowers.DoubleShot] > 0)
            {
                ShootProjectile(-30f);
                ShootProjectile(30f);
            }
            else
                ShootProjectile(0);
            _lastShot = Time.time;
        }
        return true;
    }

    private void ShootProjectile(float angle)
    {
        if (powers[(int)MachineGunPowers.Fireballs] > 0)
        {
            Vector2 dir = firingPoint.up;
            dir = dir.Rotate(angle + Random.Range(-randomSpreadAngle.Value, randomSpreadAngle.Value));
            MG_Fireball fireball = Instantiate(fireballPrefab, firingPoint.position, Quaternion.LookRotation(Vector3.forward, dir));
            fireball.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            fireball.machinegun = this;
            fireball.radius = fireballSplash.Value;
        }
        else
        {
            Vector2 dir = firingPoint.up;
            dir = dir.Rotate(angle + Random.Range(-randomSpreadAngle.Value, randomSpreadAngle.Value));
            Bullet newBullet = Instantiate(bulletPrefab, firingPoint.position, Quaternion.LookRotation(Vector3.forward, dir));
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newBullet.machinegun = this;
        }
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

        if (power == MachineGunPowers.Fireballs)
        {

        }
    }

    protected override void Setup()
    {
        bulletPrefab = attackPrefab.GetComponent<Bullet>();
        powers = new int[Enum.GetNames(typeof(MachineGunPowers)).Length];
    }

    private enum MachineGunPowers {
        DoubleShot,
        ExtraShot,
        SecondGun,
        //ArmourPierce,
        //EnemiesExplode,
        //ArmourStrip,
        Fireballs,
        //InstantFreeze,
    }
}