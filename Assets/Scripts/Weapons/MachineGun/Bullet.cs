using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public MachineGun weapon;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterTime(10f));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.otherCollider.GetComponent<Enemy>();
        if (enemy)
            weapon.BulletHit(this, enemy);
    }
}
