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
        UpdatePos();
    }

    public float Angle
    {
        get { return _angle; }
        set
        {
            _angle = value;
            UpdatePos();
        }
    }
    public float Height
    {
        get { return _height; }
        set
        {
            _height = Mathf.Max(1, value);
            UpdatePos();
        }
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

    private void UpdatePos()
    {
        transform.position = GetPos(_angle, _height);
        transform.rotation = Quaternion.Euler(0, 0, _angle);
    }
    public static Vector2 GetPos(float angle)
    {
        return GetPos(angle, 0);
    }

    public static Vector2 GetPos(float angle, float height)
    {
        return new Vector2(
            torusOrigin.x + (Mathf.Cos(Mathf.Deg2Rad * angle) * torusScale.x * (height + 1)),
            torusOrigin.y + (Mathf.Sin(Mathf.Deg2Rad * angle) * torusScale.y * (height + 1))
        );
    }
}