using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : TorusMotion
{
    public int weaponIndex = 0;

    public ModifiableFloat moveSpeed;
    public ModifiableFloat aimingMult;

    [HideInInspector]
    public float _adjustedMoveSpeed;
    [HideInInspector]
    public float _adjustedAimingMult;

    public float baseFireRate = 1f;
    public float baseBasicDamage = 0f;

    [HideInInspector]
    public float _adjustedFireRate;
    [HideInInspector]
    public float _adjustedBasicDamage;

    protected float _lastShot = 0f;

    public SpriteRenderer weaponRenderer;

    public GameObject attackPrefab;

    public Transform firingPoint;

    public List<Ability> unlockedAbilities = new List<Ability>();

    /*
    public delegate void EnemyHitEvent(Weapon origin, Enemy target);
    public EnemyHitEvent EnemyHit;

    public delegate void WeaponFiredEvent(Weapon origin);
    public WeaponFiredEvent WeaponFired;
    */

    void Start()
    {
        Setup();
    }

    protected bool _firing = false;
    protected float _actualMoveSpeed;

    void FixedUpdate()
    {
        if (PauseManager.Paused)
            return;

        _actualMoveSpeed = _adjustedMoveSpeed;

        //Shoot
        if (Input.GetButton("Fire" + weaponIndex))
        {
            _firing = true;
            _actualMoveSpeed = _adjustedMoveSpeed * _adjustedAimingMult;
            Fire();
        }
        else
            _firing = false;

        //Move
        float input = Input.GetAxis("Horizontal" + weaponIndex);
        if (input > 0)
        {
            MoveAround(-baseMoveSpeed * Time.fixedDeltaTime);
        }
        if (input < 0)
        {
            MoveAround(baseMoveSpeed * Time.fixedDeltaTime);
        }


    }

    protected abstract void Setup();

    protected abstract void Fire();

    public virtual void ChangeStat(string statName, StatChangeOperation operation, float newValue)
    {
        switch (statName.ToLower())
        {
            case "movespeed":
                ChangeFloat(ref _adjustedMoveSpeed, newValue, operation);
                break;
            case "aimingmult":
                ChangeFloat(ref _adjustedAimingMult, newValue, operation);
                break;
            case "firerate":
                ChangeFloat(ref _adjustedFireRate, newValue, operation);
                break;
        }
    }

    protected void ChangeFloat(ref float original, float newValue, StatChangeOperation operation)
    {
        if (operation == StatChangeOperation.Add)
            original = original  + newValue;
        if (operation == StatChangeOperation.Multiply)
            original = original * newValue;

        original = newValue;
    }

    protected int ChangeInt(int original, int newValue, StatChangeOperation operation)
    {
        if (operation == StatChangeOperation.Add)
            return original + newValue;
        if (operation == StatChangeOperation.Multiply)
            return original * newValue;

        return newValue;
    }

    protected bool ChangeBool(float newValue)
    {
        return newValue > 0;
    }
}

public enum WeaponType
{
    Any,
    MachineGun,
    Railgun,
    FlameThrower,
    Laser,
    MissileLauncher,
    FreezeRay,
    BoomerangChainsaw,
    Antimatter
}