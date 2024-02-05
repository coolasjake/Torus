using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HubSettings", menuName = "ScriptableObjects/HubSettings", order = 1)]
public class HubSettings : ScriptableObject
{
    public float tileSize;
    public GameObject inputPrefab;
}
