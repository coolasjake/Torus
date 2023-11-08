using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return v.RotateRad(degrees * Mathf.Deg2Rad);
    }

    public static Vector2 RotateRad(this Vector2 v, float radians)
    {
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }
}
