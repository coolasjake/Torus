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
        [Min(1)]
        public int pointsForMainTypes = 1;
        public List<FleetType> possibleMainFleets = new List<FleetType>();

        [Min(1)]
        public int pointsForRareTypes = 1;
        public List<FleetType> possibleRareFleets = new List<FleetType>();

    }
}