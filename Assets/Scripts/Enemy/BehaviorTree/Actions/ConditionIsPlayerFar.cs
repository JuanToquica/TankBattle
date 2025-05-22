using BehaviorTree;
using UnityEngine;

public class ConditionIsPlayerFar : Node
{
    private EnemyAI enemy;

    public ConditionIsPlayerFar(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.distanceToPlayer > enemy.farDistance)
            return NodeState.SUCCESS;
        return NodeState.FAILURE;
    }
}
