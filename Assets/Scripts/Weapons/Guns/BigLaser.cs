using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BigLaser : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.Railgun;
    }

    [Header("Laser Stats")]
    [Min(0)]
    public float laserLength = 5f;
    public float laserStart = 1f;
    public ModifiableFloat laserThickness = new ModifiableFloat(1f, 0f, 100f);

    [Header("Laser Refs")]
    public LineRenderer laserRenderer;
    public Transform testAttack;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            Vector2 laserCenter = transform.position + transform.position.normalized * ((laserLength / 2f) + laserStart);
            Vector2 laserScale = new Vector2(laserThickness.Value, laserLength);
            //testAttack.position = laserCenter;
            //testAttack.localScale = laserScale;
            //testAttack.rotation = TorusMotion.RotFromAngle(Angle);
            laserRenderer.transform.localScale = new Vector3(laserThickness.Value, 1);
            Collider2D[] colliders = Physics2D.OverlapBoxAll(laserCenter, laserScale, TorusMotion.RealAngleFromAngle(Angle));

            foreach (Collider2D collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy)
                    LaserHit(enemy);
            }
            _lastShot = Time.time;
        }
        return true;
    }

    protected override void WeaponFixedUpdate()
    {
        laserRenderer.gameObject.SetActive(_firing);
    }

    public void LaserHit(Enemy enemy)
    {
        if (enemy.CheckDodge(firingPoint.transform.position))
            return;
        DefaultHit(enemy);
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "laser thickness":
                laserThickness.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        BigLaserPowers power;
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
        if (laserRenderer == null)
        {
            GameObject laser = Instantiate(attackPrefab, transform);
            laserRenderer = laser.GetComponent<LineRenderer>();
        }
        powers = new int[Enum.GetNames(typeof(BigLaserPowers)).Length];
    }

    private enum BigLaserPowers
    {
        AimLaser,
        HardeningRadiation,
    }
}
