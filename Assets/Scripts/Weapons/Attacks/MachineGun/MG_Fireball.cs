using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MG_Fireball : Bullet
{
    public float radius = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StaticRefs.SpawnExplosion(radius, transform.position);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy)
                machinegun.BulletHit(this, enemy);
        }
        Destroy(gameObject);
    }
}
