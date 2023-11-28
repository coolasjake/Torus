using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fleet", menuName = "ScriptableObjects/FleetType", order = 1)]
public class FleetType : ScriptableObject
{
    [Min(1)]
    public int difficultyPoints = 1;
    public List<EnemyGroup> Groups = new List<EnemyGroup>();

    [System.Serializable]
    public class EnemyGroup
    {
        public string name = "Group";
        public EnemyClass enemyClass = EnemyClass.swarm;
        [Min(1)]
        public int numberOfEnemies = 1;
        [Range(-180, 180)]
        public float relativeAngle = 0f;
        [Min(0f)]
        public float arrivalDelay = 0f;
    }
}
