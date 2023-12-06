using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningObj : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    public Enemy enemyA;
    public Enemy enemyB;
    private int _animationFrame = 0;

    public void SetTargets(Enemy A, Enemy B)
    {
        enemyA = A;
        enemyB = B;
        UpdateLightning();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateLightning();
    }

    private void UpdateLightning()
    {
        if (enemyA == null || enemyB == null)
            return;
        transform.position = enemyA.transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, (enemyB.transform.position - enemyA.transform.position));
        transform.rotation = Quaternion.Euler(0, 0, angle);
        float dist = Vector2.Distance(enemyA.transform.position, enemyB.transform.position);
        //print("origin: " + transform.position + ", dest: " + enemyB.transform.position + ", angle: " + angle + " diff: " + (enemyB.transform.position - enemyA.transform.position));
        spriteRenderer.size = new Vector2(dist, spriteRenderer.size.y);
    }

    public void Destroy()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
