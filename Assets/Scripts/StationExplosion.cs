using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationExplosion : MonoBehaviour
{
    public float targetScale = 20f;
    public float explodeTime = 2f;
    private float _currentScale = 0;

    private void Update()
    {
        _currentScale = Mathf.Lerp(_currentScale, targetScale, (targetScale / explodeTime) * Time.deltaTime);
        transform.localScale = new Vector3(_currentScale, _currentScale, _currentScale);
        if (_currentScale == targetScale)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Trigger");
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.Destroy();
        }
    }
}
