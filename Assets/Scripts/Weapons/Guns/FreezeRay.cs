using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FreezeRay : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.FreezeRay;
    }

    [Header("FreezeRay Stats")]
    [Min(0)]
    public float rayLength = 5f;
    public float rayStart = 1f;
    public ModifiableFloat rayAngle = new ModifiableFloat(1f, 0f, 100f);

    [Header("Laser Refs")]
    public SpriteRenderer rayRenderer;
    public Transform testAttack;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            Vector2 laserCenter = transform.position + transform.position.normalized * ((rayLength / 2f) + rayStart);
            Vector2 laserScale = new Vector2(rayAngle.Value, rayLength);
            testAttack.position = laserCenter;
            testAttack.localScale = laserScale;
            testAttack.rotation = TorusMotion.RotFromAngle(Angle);
            rayRenderer.transform.localScale = new Vector3(rayAngle.Value, 1);
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
        rayRenderer.gameObject.SetActive(_firing);
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
                rayAngle.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        FreezeRayPowers power;
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
        if (rayRenderer == null)
        {
            GameObject laser = Instantiate(attackPrefab, transform);
            rayRenderer = laser.GetComponent<SpriteRenderer>();
        }
        powers = new int[Enum.GetNames(typeof(FreezeRayPowers)).Length];
    }

    private enum FreezeRayPowers
    {
        AimLaser,
        HardeningRadiation,
    }
}
