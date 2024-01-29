using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AntimatterOrb : MonoBehaviour
{
    //public Rigidbody2D RB;
    [HideInInspector]
    public AntimatterGun antimatterGun;

    public void Launch(Vector3 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
        GetComponent<Collider2D>().enabled = true;
        StartCoroutine(DestroyAfterTime(15f));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            antimatterGun.OrbHit(this, enemy);
        }
    }
}
