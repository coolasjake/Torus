using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangChainsaw : TorusMotion
{
    [HideInInspector]
    public BoomerangLauncher boomerangLauncher;

    [HideInInspector]
    public Vector2 torusVelocity = Vector2.zero;
    public float radius = 1f;

    void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius * transform.localScale.x);
        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy)
                boomerangLauncher.BoomerangHit(this, enemy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius * transform.localScale.x);
    }
}

