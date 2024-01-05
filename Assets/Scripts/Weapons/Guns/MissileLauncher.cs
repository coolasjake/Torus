using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.MissileLauncher;
    }

    [Header("MissileLauncher Stats")]
    public ModifiableFloat missileSpeed = new ModifiableFloat(10f, 0.01f, 1000f);
    public ModifiableFloat explosionSize = new ModifiableFloat(1f, 0f, 100f);

    [Header("MissileLauncher Refs")]

    private Missile missilePrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            Missile newMissile = Instantiate(missilePrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newMissile.GetComponent<Rigidbody2D>().velocity = dir * missileSpeed.Value;
            newMissile.missileLauncher = this;
            _lastShot = Time.time;
        }
        return true;
    }

    protected override void WeaponUpdate()
    {

    }

    protected override void WeaponFixedUpdate()
    {
        
    }

    public void MissileHit(Missile missile, Enemy enemy)
    {
        if (enemy.CheckDodge(missile.transform.position))
            return;
        Explosion(missile.transform.position);
        //DefaultHit(enemy);
        Destroy(missile.gameObject);
    }

    private void Explosion(Vector2 origin)
    {
        StaticRefs.SpawnExplosion(explosionSize.Value * 5f, origin);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, explosionSize.Value);
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy)
                DefaultHit(enemy);
        }
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "bullet speed":
                missileSpeed.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        MissileLauncherPowers power;
        if (Enum.TryParse(powerName, out power))
        {
            powers[(int)power] = level;
            Debug.Log("Upgrading power: " + powerName + " to " + level);
        }
        else
            Debug.Log("Couldn't find power with name: " + powerName);
    }

    protected override void Setup()
    {
        missilePrefab = attackPrefab.GetComponent<Missile>();
        powers = new int[Enum.GetNames(typeof(MissileLauncherPowers)).Length];
    }

    private enum MissileLauncherPowers
    {
        
    }
}
