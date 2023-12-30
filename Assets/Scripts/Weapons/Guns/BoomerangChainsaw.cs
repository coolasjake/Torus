using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangChainsaw : TorusMotion
{
    [HideInInspector]
    public BoomerangLauncher boomerangLauncher;

    [HideInInspector]
    public Vector2 torusVelocity = Vector2.zero;
    [HideInInspector]
    public bool returning = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            boomerangLauncher.BoomerangHit(this, enemy);
        }
    }
}

