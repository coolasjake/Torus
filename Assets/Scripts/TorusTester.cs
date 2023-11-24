using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorusTester : MonoBehaviour
{
    public Vector2 torusScale = Vector2.one;
    public Color gizmoColour = Color.blue;
    public int gizmoSections = 360;
    public TorusMotion target;
    public Vector2 speed = Vector2.one;

    // Start is called before the first frame update
    void Awake()
    {
        TorusMotion.torusScale = torusScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;

        if (Input.GetKey(KeyCode.D))
        {
            target.MoveAround(-Time.deltaTime * speed.x);
        }
        if (Input.GetKey(KeyCode.A))
        {
            target.MoveAround(Time.deltaTime * speed.x);
        }
        if (Input.GetKey(KeyCode.W))
        {
            target.Height += Time.deltaTime * speed.y;
        }
        if (Input.GetKey(KeyCode.S))
        {
            target.Height -= Time.deltaTime * speed.y;
        }
    }

    void OnDrawGizmos()
    {
        TorusMotion.torusScale = torusScale;
        Gizmos.color = gizmoColour;
        Vector2 lastPos = TorusMotion.GetPos(0);
        for (int i = 0; i <= gizmoSections; ++i)
        {
            Vector2 newPos = TorusMotion.GetPos(i * (360f / gizmoSections));
            Gizmos.DrawLine(lastPos, newPos);
            lastPos = newPos;
        }

        for (int i = 0; i < gizmoSections / 5; ++i)
        {
            float angle = i * (360f / (gizmoSections / 5));
            Vector2 origin = TorusMotion.GetPos(angle, 0.0001f);
            Vector2 torusPoint = TorusMotion.GetPos(angle, 1);
            Vector2 space = TorusMotion.GetPos(angle, 10);

            Gizmos.DrawLine(origin, torusPoint);
            Gizmos.DrawLine(torusPoint, space);
        }
    }
}
