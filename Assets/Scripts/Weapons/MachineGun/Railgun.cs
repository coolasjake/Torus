using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railgun : Weapon
{
    [Header("Railgun Stats")]
    public ModifiableFloat bulletSpeed = new ModifiableFloat(10f, 0.01f, 1000f);

    private RailRod rodPrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + fireRate.Value)
        {
            RailRod newBullet = Instantiate(rodPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newBullet.railgun = this;
            _lastShot = Time.time;
        }
        return true;
    }

    public void RodHit(RailRod rod, Enemy enemy)
    {
        enemy.SpawnExplosion(0.5f, rod.transform.position);
        DefaultHit(enemy);
    }

    protected override void Setup()
    {
        rodPrefab = attackPrefab.GetComponent<RailRod>();
    }
}
