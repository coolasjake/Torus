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

    [Header("Flamethrower Stats")]
    public ModifiableFloat spreadAngle = new ModifiableFloat(20f, 0f, 90f);

    public ModifiableFloat flamesSpeed = new ModifiableFloat(3f, 1f, 10f);
    public ModifiableFloat flameDuration = new ModifiableFloat(1f, 0.1f, 10f);
    public ModifiableFloat flamePierce = new ModifiableFloat(0.5f, 0.1f, 3f);

    public float flameWallSlow = 0.2f;
    public float flameWallSlowDur = 0.2f;

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
        newFlame.projectileDuration = flameDuration.Value;
        Vector2 dir = firingPoint.up;
        dir = dir.Rotate(Random.Range(-spreadAngle.Value, spreadAngle.Value));
        newFlame.GetComponent<Rigidbody2D>().velocity = dir * flamesSpeed.Value;
        newFlame.flamethrower = this;
        newFlame.shrinkTime = flamePierce.Value;
    }

    public void FlameHit(Flame bullet, Enemy enemy)
    {
        if (enemy.CheckDodge(bullet.transform.position))
            return;
        if (enemiesToHit.Contains(enemy) == false)
            enemiesToHit.Add(enemy);
        //DefaultHit(enemy);
        bullet.Shrink(flamePierce.Value);

        //bullet.gameObject.SetActive(false);
        //Destroy(bullet.gameObject);
    }

    protected override void WeaponFixedUpdate()
    {
        foreach (Enemy enemy in enemiesToHit)
        {
            DefaultHit(enemy);
            if (powers[(int)FlamethrowerPowers.FlameWall] > 0)
                enemy.SlowEnemy(flameWallSlow, flameWallSlowDur);
        }
        enemiesToHit.Clear();
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "spread":
                spreadAngle.AddModifier(modifierName, value, operation);
                return;
            case "flame speed":
                flamesSpeed.AddModifier(modifierName, value, operation);
                return;
            case "flame pierce":
                flameDuration.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        FlamethrowerPowers power;
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
        powers = new int[Enum.GetNames(typeof(FlamethrowerPowers)).Length];
    }

    private enum FlamethrowerPowers {
        FlameWall,
    }
}