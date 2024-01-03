using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntimatterExplosion : MonoBehaviour
{
    public Weapon triggerWeapon;
    public Animator animator;
    public float targetSize = 0;
    public float growSpeed = 2f;

    public float minSize = 0.3f;
    public float sizeMultiplier = 1f;

    [Min(0)]
    public float testAntimatter = 10f;

    private float size = 0f;
    private float totalAntimatter = 0f;
    public float StartingAntimatter
    {
        set
        {
            targetSize = CalculateSize(value);
            totalAntimatter = value;
        }
    }

    private bool animatingExplosion = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (animatingExplosion)
            return;

        if (size >= targetSize)
        {
            animatingExplosion = true;
            animator.Play("AntimatterExplosion");
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, size * 0.5f);
        foreach (Collider2D col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy)
            {
                if (animatingExplosion)
                    enemy.ReduceHealthBy(enemy.Class == EnemyClass.tank ? totalAntimatter * 0.5f : totalAntimatter, triggerWeapon);
                else if (enemy.antimatter > 0)
                {
                    totalAntimatter += enemy.antimatter;
                    enemy.antimatter = 0;
                }
            }
        }

        if (!animatingExplosion)
        {
            targetSize = CalculateSize(totalAntimatter);

            size = Mathf.MoveTowards(size, targetSize, growSpeed * Time.fixedDeltaTime);
            transform.localScale = new Vector3(size, size, size);
        }
    }

    private float CalculateSize(float antimatter)
    {
        return (minSize + minSize * Mathf.Log(antimatter + 1f, 2f) * sizeMultiplier) * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, CalculateSize(testAntimatter));
    }
}
