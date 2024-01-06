using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BigLaser : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.Laser;
    }

    [Header("Laser Stats")]
    [Min(0)]
    public float laserLength = 5f;
    public float laserStart = 1f;
    public ModifiableFloat laserThickness = new ModifiableFloat(1f, 0f, 100f);

    [Header("Laser Refs")]
    public LineRenderer laserRenderer;
    public Transform testAttack;

    private Enemy _firstHitEnemy = null;

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

            _firstHitEnemy = null;
            float closestDist = float.PositiveInfinity;
            foreach (Collider2D collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy)
                {
                    LaserHit(enemy);
                    if (enemy.Height < closestDist)
                    {
                        _firstHitEnemy = enemy;
                        closestDist = enemy.Height;
                    }
                }
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

    protected void InfiniteLightningHit(Enemy enemy)
    {
        if (damageStats.lightning.Value == 0)
            return;

        if (enemy.LightningHit(this) == false && enemy != _firstHitEnemy)
            return;
        lightningDamageEvent?.Invoke(enemy);
        //Split to nearby enemies
        Enemy chainTarget = null;
        for (int i = 0; i < lightningSplits.Value; ++i)
        {
            chainTarget = ChooseLightningChain(enemy);
            if (chainTarget == null)
                break;
            int maxChains = Mathf.RoundToInt(lightningChains.Value);
            if (DamageEvents.LightningStats.conductiveNanites && chainTarget.nanites > 0)
                maxChains += 1;
            lightningDamageEvent?.Invoke(chainTarget);
            //Chain to enemies near split targets
            for (int j = 0; j < maxChains; ++j)
            {
                chainTarget = ChooseLightningChain(chainTarget);
                if (chainTarget == null)
                    break;
                if (DamageEvents.LightningStats.conductiveNanites && chainTarget.nanites > 0)
                    maxChains += 1;
                lightningDamageEvent?.Invoke(chainTarget);
            }
        }
        if (chainTarget == null)
            StaticRefs.SpawnLightningExplosion(enemy.Size, enemy.transform.position);
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

        switch (power)
        {
            case BigLaserPowers.InfiniteLightning:
                LightningHit = InfiniteLightningHit;
                return;
        }
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
        InfiniteLightning,
    }
}
