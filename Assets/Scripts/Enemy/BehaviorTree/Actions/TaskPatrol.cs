using BehaviorTree;
using UnityEngine;

public class TaskPatrol : Node
{
    private EnemyAI enemy;

    public TaskPatrol(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        Debug.Log("Patrullando");
        return NodeState.SUCCESS;
    }
}
