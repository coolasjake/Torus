using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{

    [Header("Machinegun Stats")]
    public ModifiableFloat randomSpreadAngle = new ModifiableFloat(10f, 0f, 90f);

    public ModifiableFloat bulletSpeed = new ModifiableFloat(10f, 0.01f, 1000f);

    private float fireRateDebit = 0;

    private Bullet bulletPrefab;

    protected override bool Fire()
    {
        if (Time.time > _lastShot + fireRate.Value)
        {
            Bullet newBullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            dir = dir.Rotate(Random.Range(-randomSpreadAngle.Value, randomSpreadAngle.Value));
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * bulletSpeed.Value;
            newBullet.machinegun = this;
            _lastShot = Time.time;
        }
        return true;
    }

    public void BulletHit(Bullet bullet, Enemy enemy)
    {
        DefaultHit(enemy);
        bullet.gameObject.SetActive(false);
        Destroy(bullet.gameObject);
    }

    protected override void Setup()
    {
        bulletPrefab = attackPrefab.GetComponent<Bullet>();
    }
}