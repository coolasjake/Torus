using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Rigidbody2D RB;
    [HideInInspector]
    public MissileLauncher missileLauncher;
    public float acceleration = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (RB == null)
            RB = GetComponent<Rigidbody2D>();
        StartCoroutine(DestroyAfterTime(5f));
    }

    private void FixedUpdate()
    {
        if (acceleration != 0)
            RB.velocity = RB.velocity + RB.velocity.normalized * acceleration * Time.fixedDeltaTime;
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
            missileLauncher.MissileHit(this, enemy);
        }
    }
}
