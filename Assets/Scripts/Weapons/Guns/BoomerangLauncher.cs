using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangLauncher : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.Railgun;
    }

    [Header("BoomerangLauncher Stats")]
    public ModifiableFloat boomerangSpeed = new ModifiableFloat(10f, 0.01f, 1000f);
    public float boomerangMaxRange = 5f;

    [Header("BoomerangLauncher Refs")]
    private BoomerangChainsaw boomerangPrefab;

    private List<BoomerangChainsaw> boomerangObjs = new List<BoomerangChainsaw>();


    protected override bool Fire()
    {
        if (boomerangObjs.Count <= 0)
        {
            BoomerangChainsaw newBoomerang = Instantiate(boomerangPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newBoomerang.torusVelocity = new Vector2(0, boomerangSpeed.Value);
            //newBoomerang.GetComponent<Rigidbody2D>().velocity = dir * boomerangSpeed.Value;
            boomerangObjs.Add(newBoomerang);
            _lastShot = Time.time;
        }
        return true;
    }

    protected override void WeaponUpdate()
    {
        foreach (BoomerangChainsaw boomerang in boomerangObjs)
        {
            Vector2 bPos = boomerang.AngleAndHeight;
            float downAcc = (boomerangSpeed.Value / boomerangMaxRange) * 0.5f;
            //if (_firing)
            //    downAcc = downAcc * 0.5f;
            float angle = TorusMotion.SignedAngle(boomerang.Angle, Angle);
            boomerang.torusVelocity.x = angle * Mathf.Abs(angle);
            boomerang.torusVelocity.y -= downAcc * Time.deltaTime;
            boomerang.AngleAndHeight = bPos + boomerang.torusVelocity * Time.deltaTime;
        }
        for (int i = 0; i < boomerangObjs.Count; ++i)
        {
            if (boomerangObjs[i].Height <= 1)
            {
                Destroy(boomerangObjs[i].gameObject);
                boomerangObjs.RemoveAt(i);
                --i;
            }
        }
    }

    public void BoomerangHit(BoomerangChainsaw boomerang, Enemy enemy)
    {
        if (enemy.CheckDodge(boomerang.transform.position))
            return;
        StaticRefs.SpawnExplosion(0.5f, boomerang.transform.position);
        DefaultHit(enemy);
    }

    public override void AddModifier(string statName, string modifierName, StatChangeOperation operation, float value)
    {
        switch (statName.ToLower())
        {
            case "boomerang speed":
                boomerangSpeed.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
    }

    public override void UnlockPower(string powerName, int level)
    {
        BoomerangLauncherPowers power;
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
        boomerangPrefab = attackPrefab.GetComponent<BoomerangChainsaw>();
        powers = new int[Enum.GetNames(typeof(BoomerangLauncherPowers)).Length];
        SetupDamageTypes();
    }

    private enum BoomerangLauncherPowers
    {
        Pong,
    }
}
