using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Flame : MonoBehaviour
{
    [HideInInspector]
    public Flamethrower flamethrower;
    public float projectileDuration = 2f;
    public float shrinkTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterTime(projectileDuration));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Shrink(shrinkTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            flamethrower.FlameHit(this, enemy);
        }
    }

    public void Shrink(float duration)
    {
        shrinkTime = duration;
        if (_shrink == null)
            _shrink = StartCoroutine(ShrinkAndDestroy());
    }

    private Coroutine _shrink;
    private IEnumerator ShrinkAndDestroy()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float startTime = Time.time;
        while (Time.time < startTime + shrinkTime)
        {
            float remainingTime = (startTime + shrinkTime) - Time.time;
            float scale = Mathf.Lerp(transform.localScale.x, 0, Time.deltaTime / remainingTime);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return wait;
        }
        Destroy(gameObject);
    }
}
