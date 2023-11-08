using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{
    [SerializeField]
    private float basePhysicalDamage = 10f;
    private float _adjustedPhysicalDamage = 0f;

    [SerializeField]
    private float baseRandomSpreadAngle = 2f;
    private float _adjustedRandomSpreadAngle;

    [SerializeField]
    private float baseBulletSpeed = 2f;
    private float _adjustedBulletSpeed;

    private float fireRateDebit = 0;

    protected override void Fire()
    {
        if (Time.time > _lastShot + baseFireRate)
        {
            GameObject newBullet = Instantiate(attackPrefab, firingPoint.position, firingPoint.rotation);
            Vector2 dir = firingPoint.up;
            dir = dir.Rotate(Random.Range(-baseRandomSpreadAngle, baseRandomSpreadAngle));
            newBullet.GetComponent<Rigidbody2D>().velocity = dir * baseBulletSpeed;
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
        enemy.Hit(_adjustedBasicDamage, DamageType.basic);
        enemy.Hit(_adjustedPhysicalDamage, DamageType.physical);
    }

    protected override void Setup()
    {
        _adjustedMoveSpeed = baseMoveSpeed;
    }
}