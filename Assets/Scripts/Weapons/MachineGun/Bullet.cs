using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public MachineGun machinegun;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterTime(5f));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.collider.GetComponent<Enemy>();
        if (enemy)
        {
            machinegun.BulletHit(this, enemy);
        }
    }
}
