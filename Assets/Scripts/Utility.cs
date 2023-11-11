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

[System.Serializable]
public class ModifiableFloat
{
    public ModifiableFloat(float Default)
    {
        defaultValue = Default;
    }
    public ModifiableFloat(float Default, float Min, float Max)
    {
        defaultValue = Default;
        min = Min;
        max = Max;
    }

    [SerializeField]
    private float defaultValue = 0f;
    private float min = 0f;
    private float max = 0f;
    private float addition = 0;
    private float multiplier = 1;

    public float Value => Mathf.Clamp((defaultValue + addition) * multiplier, min, max);

    private List<NamedFloat> additionModifiers = new List<NamedFloat>();
    private List<NamedFloat> multiplyModifiers = new List<NamedFloat>();

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
        else
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
            multiplier += mod.value;
    }
}

public struct NamedFloat
{
    public NamedFloat(string Name, float Value)
    {
        name = Name;
        value = Value;
    }

    public string name;
    public float value;
}