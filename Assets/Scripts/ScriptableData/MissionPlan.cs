using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DefaultMission", menuName = "ScriptableObjects/MissionType", order = 1)]
public class MissionPlan : ScriptableObject
{
    public List<WaveData> waves = new List<WaveData>();

    [System.Serializable]
    public class WaveData
    {
        public string name = "";
        public float waveTime = 180f;
        [Min(1)]
        public int pointsForMainTypes = 1;
        [Min(0)]
        public int pointsForRareTypes = 0;
        [Range(0f, 1f)]
        public float rareTypesStartTime = 0.0f;
        [Range(0f, 1f)]
        public float rareTypesEndTime = 1f;
        [Min(1)]
        public int numBursts = 3;
        public bool pickFleetsRandomly = true;
        public List<FleetType> fleets = new List<FleetType>();
    }
}