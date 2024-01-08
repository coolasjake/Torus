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
    public bool unstoppableMode = false;
    public float unstoppableModeSpeedMult = 0.1f;
    public float acceleration = 0.2f;
    bool laser = false;
    public bool holdForSlow = false;
    [Min(1f)]
    public float slowFactor = 2f;
    public ModifiableFloat boomerangSpeed = new ModifiableFloat(10f, 0.01f, 1000f);
    public float boomerangMaxRange = 5f;
    public ModifiableFloat boomerangSize = new ModifiableFloat(0.5f, 0.1f, 5f);
    public float rotationSpeed = 1f;

    private float _firingPointHeight = 1f;

    [Header("BoomerangLauncher Refs")]
    private BoomerangChainsaw boomerangPrefab;

    private List<BoomerangChainsaw> boomerangObjs = new List<BoomerangChainsaw>();


    protected override bool Fire()
    {
        _firingPointHeight = TorusMotion.AngleHeightFromPos(firingPoint.position).y;
        if (boomerangObjs.Count <= 0 && Time.time > _lastShot + FireRate)
        {
            boomerangObjs.Add(CreateChainsaw());
        }
        return true;
    }

    private BoomerangChainsaw CreateChainsaw()
    {
        BoomerangChainsaw newBoomerang = Instantiate(boomerangPrefab, firingPoint.position, firingPoint.rotation);
        float speed = boomerangSpeed.Value;
        if (unstoppableMode)
            speed = speed * unstoppableModeSpeedMult;
        newBoomerang.torusVelocity = new Vector2(0, speed);
        newBoomerang.boomerangLauncher = this;
        newBoomerang.transform.localScale = new Vector3(boomerangSize.Value, boomerangSize.Value, boomerangSize.Value);
        newBoomerang.RotateWithAngle = false;
        return newBoomerang;
    }

    private void ReplaceChainsaws()
    {
        int count = boomerangObjs.Count;
        for (int i = 0; i < count; ++i)
        {
            BoomerangChainsaw newBoomerang = CreateChainsaw();
            newBoomerang.torusVelocity = boomerangObjs[i].torusVelocity;
            newBoomerang.AngleAndHeight = boomerangObjs[i].AngleAndHeight;
            Destroy(boomerangObjs[i].gameObject);
            boomerangObjs.RemoveAt(i);
            --i;
        }
    }

    protected override void WeaponUpdate()
    {
        if (unstoppableMode)
            UnstoppableMode();
        else
            NormalMode();
    }

    private void NormalMode()
    {
        float downAcc = (boomerangSpeed.Value * boomerangSpeed.Value) / (2f * (boomerangMaxRange - 1f));
        foreach (BoomerangChainsaw boomerang in boomerangObjs)
        {
            Vector2 bPos = boomerang.AngleAndHeight;
            bPos.x = Angle;
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
            boomerang.transform.Rotate(0, 0, rotationSpeed);
            boomerang.laser.SetPosition(0, firingPoint.position);
            boomerang.laser.SetPosition(1, boomerang.transform.position);
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

    private void UnstoppableMode()
    {
        float speed = boomerangSpeed.Value * unstoppableModeSpeedMult;
        foreach (BoomerangChainsaw boomerang in boomerangObjs)
        {
            float bHeight = boomerang.Height;
            float angle = TorusMotion.SignedAngle(boomerang.Angle, Angle);
            //boomerang.torusVelocity.x = angle * Mathf.Abs(angle);
            if (_firing)
            {
                float distFromMax = boomerangMaxRange - boomerang.Height;
                float targetSpeed = Mathf.Min(speed, distFromMax);
                boomerang.torusVelocity.y = Mathf.MoveTowards(boomerang.torusVelocity.y, targetSpeed, speed * Time.fixedDeltaTime * acceleration);
            }
            else
            {
                float distFromOrigin = Vector2.Distance(boomerang.transform.position, firingPoint.position);
                if (boomerang.Height <= _firingPointHeight)
                    distFromOrigin = 0;
                boomerang.torusVelocity.y = Mathf.MoveTowards(boomerang.torusVelocity.y, Mathf.Max(-speed, -distFromOrigin), speed * Time.fixedDeltaTime * acceleration);
            }
            bHeight += (boomerang.torusVelocity.y + (speed * Mathf.Sin(Time.time * 5f) * 0.2f)) * Time.deltaTime;

            boomerang.AngleAndHeight = new Vector2(Angle, bHeight);
            boomerang.transform.Rotate(0, 0, rotationSpeed);
            boomerang.laser.SetPosition(0, firingPoint.position);
            boomerang.laser.SetPosition(1, boomerang.transform.position);
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
            case "boomerang size":
                boomerangSize.AddModifier(modifierName, value, operation);
                return;
        }

        base.AddModifier(statName, modifierName, operation, value);
        ReplaceChainsaws();
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
        ReplaceChainsaws();
    }

    protected override void Setup()
    {
        boomerangPrefab = attackPrefab.GetComponent<BoomerangChainsaw>();
        powers = new int[Enum.GetNames(typeof(BoomerangLauncherPowers)).Length];
    }

    private enum BoomerangLauncherPowers
    {
        UnstoppableBlade,
        Pong,
    }

    private void OnDrawGizmosSelected()
    {
        TorusTester.DrawTorusGizmo(boomerangMaxRange, 180, Color.red);
    }
}
