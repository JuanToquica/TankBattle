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
        if (enemy.distanceToPlayer < enemy.nearDistance)
            return NodeState.SUCCESS;
        enemy.followingPath = false;
        return NodeState.FAILURE;
    }
}
