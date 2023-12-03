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
            _angle = (value + 360f) % 360f;
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

    /// <summary> Move the object a fixed distance around the torus, by changing the angle relative to its height. </summary>
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

    public void ApplyTransformPos()
    {
        AngleAndHeight = AngleHeightFromPos(transform.position);
    }

    public Vector2 VectorAsAngleHeight(Vector2 vector)
    {
        return AngleHeightFromPos((Vector2)transform.position + vector) - AngleAndHeight;
    }

    private void UpdatePos()
    {
        transform.position = GetPos(_angle, _height);
        //SimpleRotation();
        AccurateRotation();
        transform.hasChanged = false;
    }

    private void SimpleRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, _angle - 90f);
    }

    private void AccurateRotation()
    {
        Vector2 scaledPoint = new Vector2(Mathf.Cos(Mathf.Deg2Rad * _angle) * torusScale.x, Mathf.Sin(Mathf.Deg2Rad * _angle) * torusScale.y);
        float angle = Vector2.SignedAngle(Vector2.right, scaledPoint);
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
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

    public static Vector2 AngleHeightFromPos(Vector2 pos)
    {
        //Set angle and height based on transform pos
        float angle = AngleFromPos(pos);
        float height = DistanceFromPos(pos);
        return new Vector2(angle, height);
    }

    public static float AngleFromPos(Vector2 pos)
    {
        return (Vector2.SignedAngle(Vector2.right, ((Vector2)pos / torusScale) - torusOrigin) + 360f) % 360f;
    }

    public static float DistanceFromPos(Vector2 pos)
    {
        return Vector2.Distance(torusOrigin, pos / torusScale);
    }    
}