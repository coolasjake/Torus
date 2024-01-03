using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flamethrower : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.FlameThrower;
    }

    [Header("Machinegun Stats")]
    public ModifiableFloat spreadAngle = new ModifiableFloat(20f, 0f, 90f);

    public float flamesSpeed = 3f;
    public float flameDuration = 3f;

    //private float fireRateDebit = 0;

    private Flame flamePrefab;

    private List<Enemy> enemiesToHit = new List<Enemy>();

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            SpawnFlame();
            SpawnFlame();
            SpawnFlame();
            _lastShot = Time.time;
        }
        return true;
    }

    private void SpawnFlame()
    {
        Flame newFlame = Instantiate(flamePrefab, firingPoint.position, firingPoint.rotation);
        newFlame.projectileDuration = flameDuration;
        Vector2 dir = firingPoint.up;
        dir = dir.Rotate(Random.Range(-spreadAngle.Value, spreadAngle.Value));
        newFlame.GetComponent<Rigidbody2D>().velocity = dir * flamesSpeed;
        newFlame.flamethrower = this;
    }

    public void FlameHit(Flame bullet, Enemy enemy)
    {
        if (enemy.CheckDodge(bullet.transform.position))
            return;
        if (enemiesToHit.Contains(enemy) == false)
            enemiesToHit.Add(enemy);
        //DefaultHit(enemy);
        bullet.Shrink(0.5f);

        //bullet.gameObject.SetActive(false);
        //Destroy(bullet.gameObject);
    }

    protected override void WeaponFixedUpdate()
    {
        foreach (Enemy enemy in enemiesToHit)
            DefaultHit(enemy);
        enemiesToHit.Clear();
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "spread":
                spreadAngle.AddModifier(modifierName, value, operation);
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
        flamePrefab = attackPrefab.GetComponent<Flame>();
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