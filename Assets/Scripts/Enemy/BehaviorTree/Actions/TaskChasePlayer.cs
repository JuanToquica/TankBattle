using BehaviorTree;
using UnityEngine;

public class TaskChasePlayer : Node
{
    private EnemyAI enemy;

    public TaskChasePlayer(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        Debug.Log("Persiguiendo jugador");
        return NodeState.SUCCESS;
    }
}
