using BehaviorTree;
using UnityEngine;

public class ConditionIsPlayerNearby : Node
{
    private EnemyAI enemy;
    public ConditionIsPlayerNearby(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.distanceToPlayer < enemy.enemyAIParameters.nearDistance)
            return NodeState.SUCCESS;
        return NodeState.FAILURE;
    }
}
