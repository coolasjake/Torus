using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public MissionData missionData;

    private List<EnemyFleet> fleets = new List<EnemyFleet>();

    public void StartWave()
    {

    }

    private void PlanWave()
    {

    }

    private class EnemyFleet
    {
        public float startTime = 0f;
        public List<EnemyData> enemies = new List<EnemyData>();
        public List<EnemyType> types = new List<EnemyType>();
        public float angle = 0f;
    }
}
