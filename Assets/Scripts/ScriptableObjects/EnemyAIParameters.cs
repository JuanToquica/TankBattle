using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyAIParameters", menuName = "ScriptableObject/EnemyAIParameters")]
public class EnemyAIParameters : ScriptableObject
{
    public float timeToLeaveSpawn;
    public float timeToForgetPlayer;
    public float distanceToDetectPlayer;
    public float farDistance;
    public float nearDistance;
    public float maxAimingTolerance;
}
