using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoomerangLauncher : Weapon
{
    public override WeaponType Type()
    {
        return WeaponType.BoomerangChainsaw;
    }

    [Header("BoomerangLauncher Stats")]
    public bool holdForSlow = false;
    [Min(1f)]
    public float slowFactor = 2f;
    public ModifiableFloat boomerangSpeed = new ModifiableFloat(10f, 0.01f, 1000f);
    public float boomerangMaxRange = 5f;
    public ModifiableFloat boomerangSize = new ModifiableFloat(0.5f, 0.1f, 5f);

    [Header("BoomerangLauncher Refs")]
    private BoomerangChainsaw boomerangPrefab;

    private List<BoomerangChainsaw> boomerangObjs = new List<BoomerangChainsaw>();


    protected override bool Fire()
    {
        if (boomerangObjs.Count <= 0 && Time.time > _lastShot + FireRate)
        {
            BoomerangChainsaw newBoomerang = Instantiate(boomerangPrefab, firingPoint.position, firingPoint.rotation);
            newBoomerang.torusVelocity = new Vector2(0, boomerangSpeed.Value);
            newBoomerang.boomerangLauncher = this;
            newBoomerang.transform.localScale = new Vector3(boomerangSize.Value, boomerangSize.Value, boomerangSize.Value);
            boomerangObjs.Add(newBoomerang);
        }
        return true;
    }

    protected override void WeaponUpdate()
    {
        foreach (BoomerangChainsaw boomerang in boomerangObjs)
        {
            Vector2 bPos = boomerang.AngleAndHeight;
            bPos.x = Angle;
            float downAcc = (boomerangSpeed.Value * boomerangSpeed.Value) / boomerangMaxRange;
            float angle = TorusMotion.SignedAngle(boomerang.Angle, Angle);
            //boomerang.torusVelocity.x = angle * Mathf.Abs(angle);
            if (_firing ^ holdForSlow)
            {
                boomerang.torusVelocity.y -= downAcc * Time.deltaTime;
                bPos += boomerang.torusVelocity * Time.deltaTime;
            }
            else
            {
                boomerang.torusVelocity.y -= downAcc * Time.deltaTime / slowFactor;
                bPos += boomerang.torusVelocity * Time.deltaTime / slowFactor;
            }

            boomerang.AngleAndHeight = bPos;
        }
        for (int i = 0; i < boomerangObjs.Count; ++i)
        {
            if (boomerangObjs[i].Height <= 1)
            {
                Destroy(boomerangObjs[i].gameObject);
                boomerangObjs.RemoveAt(i);
                --i;
                _lastShot = Time.time;
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
    }

    private enum BoomerangLauncherPowers
    {
        Pong,
    }
}
