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
    void Start()
    {
        TorusMotion.torusScale = torusScale;
    }

    // Update is called once per frame
    void Update()
    {
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
    }
}
