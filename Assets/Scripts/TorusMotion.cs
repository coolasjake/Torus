using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorusMotion : MonoBehaviour
{
    public static Vector2 torusOrigin = Vector2.zero;
    public static Vector2 torusScale = Vector2.one;

    private float _angle = 90f;
    private float _height = 1f;

    void Awake()
    {
        ApplyTransformPos();
    }

    public float Angle
    {
        get { return _angle; }
        set
        {
            _angle = value % 360f;
            UpdatePos();
        }
    }

    public float Height
    {
        get { return _height; }
        set
        {
            _height = Mathf.Max(1f, value);
            UpdatePos();
        }
    }

    public void MoveCloser(float dist)
    {
        Height -= dist;
    }

    public void MoveAround(float dist)
    {
        Angle += dist / (2 * _height);
    }

    public Vector2 AngleAndHeight
    {
        get { return new Vector2(_angle, _height); }
        set
        {
            _angle = value.x;
            _height = value.y;
            UpdatePos();
        }
    }

    protected void ApplyTransformPos()
    {
        //Set angle and height based on transform pos
        float angle = Vector2.SignedAngle(Vector2.right, ((Vector2)transform.position / torusScale) - torusOrigin);
        float height = Vector2.Distance(torusOrigin, transform.position / torusScale);
        AngleAndHeight = new Vector2(angle, height);
    }

    private void UpdatePos()
    {
        transform.position = GetPos(_angle, _height);
        transform.rotation = Quaternion.Euler(0, 0, _angle - 90f);
        transform.hasChanged = false;
    }
    public static Vector2 GetPos(float angle)
    {
        return GetPos(angle, 1);
    }

    public static Vector2 GetPos(float angle, float height)
    {
        Vector2 worldPos = new Vector2(
            torusOrigin.x + (Mathf.Cos(Mathf.Deg2Rad * angle) * torusScale.x * (height)),
            torusOrigin.y + (Mathf.Sin(Mathf.Deg2Rad * angle) * torusScale.y * (height))
        );
        return worldPos;
    }
}