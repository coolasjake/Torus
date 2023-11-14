using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{

    [Header("Machinegun Stats")]
    public ModifiableFloat damage = new ModifiableFloat(10f, 0f, 1000f);

    public ModifiableFloat randomSpreadAngle = new ModifiableFloat(10f, 0f, 90f);

    public ModifiableFloat bulletSpeed = new ModifiableFloat(10f, 0.01f, 1000f);

    private float fireRateDebit = 0;

    private Bullet bulletPrefab;

    protected override void Fire()
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
        /*
        fireRateDebit += Time.fixedDeltaTime;
        while (fireRateDebit > Mathf.Max(0.01f, baseFireRate))
        {
            fireRateDebit -= _adjustedFireRate;
            GameObject newBullet = Instantiate(attackPrefab, firingPoint.position, firingPoint.rotation);
            newBullet.GetComponent<Rigidbody2D>().velocity = firingPoint.up * baseBulletSpeed;
        }
        */
    }

    public void BulletHit(Bullet bullet, Enemy enemy)
    {
        print("hit " + enemy.name);
        DefaultHit(enemy);
        bullet.gameObject.SetActive(false);
        Destroy(bullet.gameObject);
    }

    protected override void Setup()
    {
        bulletPrefab = attackPrefab.GetComponent<Bullet>();
    }
}