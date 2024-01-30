using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public static class Utility
{
    /// <summary> Rotate the vector2 using degrees. </summary>
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return v.RotateRad(degrees * Mathf.Deg2Rad);
    }

    /// <summary> Rotate the vector2 using radians. </summary>
    public static Vector2 RotateRad(this Vector2 v, float radians)
    {
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }

    /// <summary> Return true if this float has a greater unsigned value than the comparison float. </summary>
    public static bool FurtherFromZero(this float f, float comparison)
    {
        if ((comparison > 0 && f > comparison) || (comparison < 0 && f < comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this int has a greater unsigned value than the comparison int. </summary>
    public static bool FurtherFromZero(this int f, int comparison)
    {
        if ((comparison > 0 && f > comparison) || (comparison < 0 && f < comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this float has a smaller unsigned value than the comparison float. </summary>
    public static bool CloserToZero(this float f, float comparison)
    {
        if ((comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison))
            return true;
        return false;
    }

    /// <summary> Return true if this int has a smaller unsigned value than the comparison int. </summary>
    public static bool CloserToZero(this int f, int comparison)
    {
        if ((comparison > 0 && f <= comparison) || (comparison < 0 && f >= comparison))
            return true;
        return false;
    }

    public static float UnsignedDifference(float f, float comparison)
    {
        return Mathf.Abs(f - comparison);
    }

    public static float Sign(this float signOf)
    {
        if (signOf > 0)
            return 1;
        else if (signOf == 0)
            return 0;
        return -1;
    }

    public static bool Outside(this float value, float lowerBounds, float upperBounds)
    {
        if (value < lowerBounds || value > upperBounds)
            return true;
        return false;
    }

    public static bool Outside(this int value, int lowerBounds, int upperBounds)
    {
        if (value < lowerBounds || value > upperBounds)
            return true;
        return false;
    }

    /// <summary> Return true if the integer is within the specified bounds [inclusive]. </summary>
    public static bool Inside(this int value, int lowerBounds, int upperBounds)
    {
        if (value >= lowerBounds && value <= upperBounds)
            return true;
        return false;
    }

    /// <summary> Return true if the float is within the specified bounds [exclusive]. </summary>
    public static bool Inside(this float value, float lowerBounds, float upperBounds)
    {
        if (value > lowerBounds && value < upperBounds)
            return true;
        return false;
    }

    public static bool Inside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Inside(lowerBounds.x, upperBounds.x) &&
            value.y.Inside(lowerBounds.y, upperBounds.y) &&
            value.z.Inside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static bool Outside(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.y.Outside(lowerBounds.y, upperBounds.y) ||
            value.z.Outside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static bool Outside2D(this Vector3 value, Vector3 lowerBounds, Vector3 upperBounds)
    {
        if (value.x.Outside(lowerBounds.x, upperBounds.x) ||
            value.z.Outside(lowerBounds.z, upperBounds.z))
            return true;
        return false;
    }

    public static int Lerp(this int value, int target, int t)
    {
        if (value < target)
        {
            value += Mathf.Abs(t);
            if (value >= target)
                return target;
            else
                return value;
        }
        else
        {
            value -= Mathf.Abs(t);
            if (value <= target)
                return target;
            else
                return value;
        }
    }

    public static float Lerp(this float value, float target, float t)
    {
        if (value < target)
        {
            value += Mathf.Abs(t);
            if (value >= target)
                return target;
            else
                return value;
        }
        else
        {
            value -= Mathf.Abs(t);
            if (value <= target)
                return target;
            else
                return value;
        }
    }

    public static Vector3 Invert(this Vector3 value)
    {
        return new Vector3(-value.x, -value.y, -value.z);
    }

    /// <summary> Replace the Y axis of this vector with the specified value. </summary>
    public static Vector3 FixedY(this Vector3 value, float newY)
    {
        return new Vector3(value.x, newY, value.z);
    }

    /// <summary> Set the specified axis value to zero. </summary>
    public static Vector3 DeleteAxis(this Vector3 value, int axis)
    {
        value[axis] = 0;
        return value;
    }

    /// <summary> Set the specified axis value to zero. </summary>
    public static Vector3 DeleteAxis(this Vector3 value, Axis axis)
    {
        value[AxisToInt(axis)] = 0;
        return value;
    }

    public static int AxisToInt(Axis axis)
    {
        if (axis == Axis.X)
            return 0;
        else if (axis == Axis.Z)
            return 2;
        else
            return 1;
    }

    public static Axis IntToAxis(int axisIndex)
    {
        if (axisIndex == 0)
            return Axis.X;
        else if (axisIndex == 2)
            return Axis.Z;
        else
            return Axis.Y;
    }

    /// <summary> Find the largest component of the vector and return it as an int (e.g. 0 = x is the axis). </summary>
    public static int GetBiggestAxisIndexOfVector(Vector3 vector)
    {
        int largestIndex = 0;
        for (int i = 0; i < 3; i++)
        {
            largestIndex = Mathf.Abs(vector[i]) > Mathf.Abs(vector[largestIndex]) ? i : largestIndex;
        }
        return largestIndex;
    }

    /// <summary> Find the largest component of the vector and return it as an int (e.g. 0 = x is the axis). </summary>
    public static Axis GetAxisFromVector(Vector3 vector)
    {
        return IntToAxis(GetBiggestAxisIndexOfVector(vector));
    }

    public static Vector3 MultipliedBy(this Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static Vector3Int MultipliedBy(this Vector3Int v1, Vector3 v2)
    {
        return new Vector3Int(Mathf.FloorToInt(v1.x * v2.x), Mathf.FloorToInt(v1.y * v2.y), Mathf.FloorToInt(v1.z * v2.z));
    }

    /// <summary> Change a vector2 into a vector 3 as if the V2 is horizontal instead of vertical (so that the y value becomes the z value). </summary>
    public static Vector3 ToVector3(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static bool WithinRange(Vector3 pointA, Vector3 pointB, float range)
    {
        return (Vector3.SqrMagnitude(pointA - pointB) < range * range);
    }

    public static bool IsSameAs(this Vector3 vectorA, Vector3 vectorB, int accuracyMultiplier)
    {
        bool x = (int)(vectorA.x * accuracyMultiplier) == (int)(vectorB.x * accuracyMultiplier);
        bool y = (int)(vectorA.y * accuracyMultiplier) == (int)(vectorB.y * accuracyMultiplier);
        bool z = (int)(vectorA.z * accuracyMultiplier) == (int)(vectorB.z * accuracyMultiplier);

        return x && y && z;
    }

    public static bool LargerThan(this Vector3 vector, float magnitude)
    {
        return (vector.sqrMagnitude > magnitude * magnitude);
    }

    public static bool SmallerThan(this Vector3 vector, float magnitude)
    {
        return (vector.sqrMagnitude < magnitude * magnitude);
    }

    public static Vector3 NearestPointOnInfiniteLine(this Vector3 point, Vector3 origin, Vector3 direction)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }

    public static Vector3 NearestPointOnFiniteLine(this Vector3 point, Vector3 start, Vector3 end)
    {
        //Get heading
        Vector3 heading = (end - start);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - start;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return start + heading * dotP;
    }

    public static Vector3 StartingVelOfCollision(Rigidbody body, Collision collision)
    {
        Vector3 impulse = collision.impulse;

        if (Vector3.Dot(collision.GetContact(0).normal, impulse) < 0f)
            impulse *= -1f;

        return body.velocity - impulse / body.mass;
    }

    public static List<int> CreateIndexList(int count)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < count; ++i)
            list.Add(i);
        return list;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public static T Last<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);
        return list[list.Count - 1];
    }

    public static T First<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);
        return array[0];
    }

    public static T Last<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);
        return array[array.Length - 1];
    }

    public static T First<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);
        return list[0];
    }

    public static T Random<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static T Random<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static string ElementsToString<T>(this List<T> list)
    {
        string text = "";
        foreach (T element in list)
        {
            text += element.ToString() + ", ";
        }
        return text;
    }

    public static Matrix4x4 GenerateMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetTRS(position, rotation, scale);
        return matrix;
    }

    public static Color MultiplySaturation(this Color oldCol, float saturationMultiplier)
    {
        float h, s, v = 0;
        Color.RGBToHSV(oldCol, out h, out s, out v);
        return Color.HSVToRGB(h, s * saturationMultiplier, v);
    }

    public static Color ChangeHue(this Color oldCol, float newHue)
    {
        float h, s, v = 0;
        Color.RGBToHSV(oldCol, out h, out s, out v);
        return Color.HSVToRGB(newHue / 360f, s, v);
    }

    public static Color WithAlpha(this Color oldCol, float alpha)
    {
        oldCol.a = alpha;
        return oldCol;
    }

    public static float Duration(this AnimationCurve curve)
    {
        if (curve == null)
            return 0;

        if (curve.length == 0)
            return 0;

        return curve.keys[curve.length - 1].time;
    }

    public static float DecimalPlaces(this double value, int numPlaces)
    {
        numPlaces = Mathf.Clamp(numPlaces, 0, 10);
        float multiplier = Mathf.Pow(10, numPlaces);
        return Mathf.Round((float)value * multiplier) / multiplier;
    }

    public static float DecimalPlaces(this float value, int numPlaces)
    {
        numPlaces = Mathf.Clamp(numPlaces, 0, 10);
        float multiplier = Mathf.Pow(10, numPlaces);
        return Mathf.Round(value * multiplier) / multiplier;
    }

    public static string SecondsToTime(float seconds)
    {
        return SecondsToTime(seconds, false, true, true, false);
    }

    public static string SecondsToTime(float seconds, bool showMili = false, bool showSeconds = true, bool showMinutes = true, bool showHours = false)
    {
        List<int> units = new List<int>();

        if (showMili)
            units.Add(Mathf.FloorToInt(((seconds - (int)seconds)) * 100));
        if (showSeconds)
            units.Add(Mathf.FloorToInt(seconds % 60));
        if (showMinutes)
            units.Add((Mathf.FloorToInt(seconds / 60) % 60));
        if (showHours)
            units.Add(Mathf.FloorToInt(seconds / 60 / 60));

        if (units.Count == 0)
            return "";

        string time = "";
        for (int i = 0; i < units.Count - 1; ++i)
            time = ":" + units[i].ToString("00") + time;
        time = units[units.Count - 1].ToString() + time;

        return time;
    }
}

/// <summary> Stores modifiers to a stat, and calculates the value every time one is added or removed using the function (default * multipliers * (percentage * 0.01) + additions).
/// Default is set in inspector, multipliers multiply together (e.g. 3 & 3 = 9), percentages and additions add together (e.g. 3% & 3% = 6%, +3 & +3 = +6)</summary>
[System.Serializable]
public class ModifiableFloat
{
    public ModifiableFloat(float Default)
    {
        defaultValue = Default;
        _value = CalculateValue;
    }
    public ModifiableFloat(float Default, float Min, float Max)
    {
        defaultValue = Default;
        min = Min;
        max = Max;
        CalculateModifierSums();
    }

    [SerializeField]
    private float defaultValue = 0f;
    [SerializeField]
    private float min = float.NegativeInfinity;
    [SerializeField]
    private float max = float.PositiveInfinity;
    [SerializeField]
    private float addition = 0;
    [SerializeField]
    private float multiplier = 1;
    [SerializeField]
    private float percentage = 100;
    [SerializeField]
    private float _value = 0;

    public float Value
    {
        get
        {
            _value = CalculateValue;
            return _value;
        }
     }

    private List<NamedFloat> additionModifiers = new List<NamedFloat>();
    private List<NamedFloat> multiplyModifiers = new List<NamedFloat>();
    private List<NamedFloat> percentageModifiers = new List<NamedFloat>();

    public void AddModifier(string name, float value, StatChangeOperation operation)
    {
        if (operation == StatChangeOperation.Add)
        {
            int index = additionModifiers.FindIndex(X => X.name == name);
            if (index == -1)
                additionModifiers.Add(new NamedFloat(name, value));
            else
                additionModifiers[index] = new NamedFloat(name, value);
        }
        else if (operation == StatChangeOperation.Multiply)
        {
            int index = multiplyModifiers.FindIndex(X => X.name == name);
            if (index == -1)
                multiplyModifiers.Add(new NamedFloat(name, value));
            else
                multiplyModifiers[index] = new NamedFloat(name, value);
        }
        else if (operation == StatChangeOperation.Percentage)
        {
            int index = percentageModifiers.FindIndex(X => X.name == name);
            if (index == -1)
                percentageModifiers.Add(new NamedFloat(name, value));
            else
                percentageModifiers[index] = new NamedFloat(name, value);
        }
        else //Set
            defaultValue = value;

        CalculateModifierSums();
    }

    private void CalculateModifierSums()
    {
        addition = 0;
        foreach (NamedFloat mod in additionModifiers)
            addition += mod.value;

        multiplier = 1;
        foreach (NamedFloat mod in multiplyModifiers)
            multiplier *= mod.value;

        percentage = 100;
        foreach (NamedFloat mod in percentageModifiers)
            percentage += mod.value;

        _value = CalculateValue;
    }

    private float CalculateValue => Mathf.Clamp(defaultValue * multiplier * (percentage * 0.01f) + addition, min, max);

    public static float GenerateModifiedValue(float defaultValue, float multiplier, float percentage, float addition, float min, float max)
    {
        return Mathf.Clamp(defaultValue * multiplier * (percentage * 0.01f) + addition, min, max);
    }
}
[System.Serializable]
public class NamedFloat
{
    public NamedFloat(string Name, float Value)
    {
        name = Name;
        value = Value;
    }

    public string name;
    public float value;
}