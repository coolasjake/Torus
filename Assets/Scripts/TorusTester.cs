using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TorusTester : MonoBehaviour
{
    public float gizmoHeight = 1f;
    public Color gizmoColour = Color.blue;
    public int gizmoSections = 360;
    public TorusMotion targetA;
    public TorusMotion targetB;
    public Vector2 speed = Vector2.one;
    public EnemySpawner spawner;

    private WeaponInput input;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && targetA != null && targetB != null)
            print(TorusMotion.SignedAngle(targetA.Angle, targetB.Angle));


        if (targetA == null)
            return;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColour;
        Vector2 lastPos = TorusMotion.GetPos(0);
        for (int i = 0; i <= gizmoSections; ++i)
        {
            Vector2 newPos = TorusMotion.GetPos(i * (360f / gizmoSections), gizmoHeight);
            Gizmos.DrawLine(lastPos, newPos);
            lastPos = newPos;
        }

        for (int i = 0; i < gizmoSections / 5; ++i)
        {
            float angle = i * (360f / (gizmoSections / 5));
            Vector2 origin = TorusMotion.torusOrigin;
            Vector2 torusPoint = TorusMotion.GetPos(angle, 1);
            Vector2 space = TorusMotion.GetPos(angle, 10);

            Gizmos.DrawLine(origin, torusPoint);
            Gizmos.DrawLine(torusPoint, space);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawner != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawner.enemySpacing[0]);
            Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(spawner.enemySpacing[0]));
            Gizmos.DrawWireSphere(transform.position, Mathf.Pow(spawner.enemySpacing[0], 2));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawner.enemySpacing[1]);
            Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(spawner.enemySpacing[1]));
            Gizmos.DrawWireSphere(transform.position, Mathf.Pow(spawner.enemySpacing[1], 2));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawner.enemySpacing[2]);
            Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(spawner.enemySpacing[2]));
            Gizmos.DrawWireSphere(transform.position, Mathf.Pow(spawner.enemySpacing[2], 2));
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawner.enemySpacing[3]);
            Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(spawner.enemySpacing[3]));
            Gizmos.DrawWireSphere(transform.position, Mathf.Pow(spawner.enemySpacing[3], 2));
            Gizmos.color = Color.green;
        }
    }
}
