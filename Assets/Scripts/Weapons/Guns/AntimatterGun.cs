using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AntimatterGun : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.Antimatter;
    }

    [Header("Antimatter Gun Stats")]
    public ModifiableFloat maxCharge = new ModifiableFloat(2f, 1f, 20f);
    [Range(0f, 1f)]
    public float chargePercentToFire = 0.5f;
    public ModifiableFloat chargeRate = new ModifiableFloat(1f, 0.1f, 20f);
    public ModifiableFloat orbSpeed = new ModifiableFloat(1f, 0.1f, 20f);
    public ModifiableFloat baseAntimatter = new ModifiableFloat(10f, 1f, 50f);
    public float minOrbSize = 0.7f;
    public float maxOrbSize = 1f;

    private AntimatterOrb _orbBeingCharged;
    private float _currentCharge = 0f;

    [Header("Antimatter Gun Refs")]
    public AntimatterOrb orbPrefab;
    public Transform laserOrigin;


    protected override bool Fire()
    {
        if (Time.time > _lastShot + FireRate)
        {
            //Increase charge, fire if released
            ChargeOrb();
        }
        return true;
    }

    private void CreateOrb()
    {
        _orbBeingCharged = Instantiate(orbPrefab, firingPoint.position, firingPoint.rotation);
        _orbBeingCharged.antimatterGun = this;
        _currentCharge = 0;
    }

    private void ChargeOrb()
    {
        if (_orbBeingCharged == null)
            CreateOrb();
        _currentCharge += chargeRate.Value * Time.fixedDeltaTime;
        _currentCharge = Mathf.Min(_currentCharge, maxCharge.Value);

        ShowOrbCharge();
    }

    private void ShowOrbCharge()
    {
        float s = minOrbSize + ((_currentCharge / maxCharge.Value) * (maxOrbSize - minOrbSize));
        _orbBeingCharged.transform.localScale = new Vector3(s, s, s);
        _orbBeingCharged.transform.position = firingPoint.position;

        //pitch of sound?
    }

    private void ReleaseOrb()
    {
        if (_orbBeingCharged == null)
            return;

        if (_currentCharge >= maxCharge.Value * chargePercentToFire)
        {
            _orbBeingCharged.Launch(firingPoint.up * orbSpeed.Value);
            _orbBeingCharged = null;
            _lastShot = Time.time;
        }
        else
        {
            _currentCharge -= chargeRate.Value * Time.fixedDeltaTime;
            _currentCharge = Mathf.Max(_currentCharge, 0);
            ShowOrbCharge();
        }
    }

    public void OrbHit(AntimatterOrb orb, Enemy enemy)
    {
        if (enemy.CheckDodge(orb.transform.position) == false)
        {
            AntimatterExplosion explosion = StaticRefs.SpawnAntimatterExplosion(orb.transform.position, enemy);
            explosion.CollectAntimatter = CollectAntimatter;
            explosion.DealExplosionDamage = AntimatterExplosionHit;
            explosion.totalAntimatter = baseAntimatter.Value;
            explosion.CollectAntimatter.Invoke(explosion, enemy);

            orb.gameObject.SetActive(false);
            Destroy(orb.gameObject);
        }
    }

    public void AntimatterExplosionHit(AntimatterExplosion explosion, Enemy enemy)
    {
        enemy.lastHitBy = this;
        enemy.ReduceHealthBy(enemy.Class == EnemyClass.tank ? explosion.totalAntimatter * 0.5f : explosion.totalAntimatter);
        enemy.matterCollected = false;
    }

    public void CollectAntimatter(AntimatterExplosion explosion, Enemy enemy)
    {
        explosion.totalAntimatter += enemy.antimatter;
        if (enemy.matterCollected == false)
        {
            explosion.totalAntimatter += enemy.Size * damageStats.antimatter.Value;
            enemy.matterCollected = true;
        }
        enemy.antimatter = 0;
        enemy.nanites = 0;
        enemy.acid = 0;
    }

    protected override void WeaponUpdate()
    {
        if (_firing == false)
        {
            ReleaseOrb();
        }
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "charge rate":
                chargeRate.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        AntimatterGunPowers power;
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
        orbPrefab = attackPrefab.GetComponent<AntimatterOrb>();
        powers = new int[Enum.GetNames(typeof(AntimatterGunPowers)).Length];
    }

    private enum AntimatterGunPowers
    {
        UnstoppableBlade,
        Pong,
    }
}
