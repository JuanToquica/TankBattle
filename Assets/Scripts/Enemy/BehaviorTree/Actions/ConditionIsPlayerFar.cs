using BehaviorTree;
using UnityEngine;

public class ConditionIsPlayerFar : Node
{
    private EnemyAI enemy;
    private Transform player;
    private Transform turret;

    public ConditionIsPlayerFar(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.distanceToPlayer > enemy.stoppingDistance)
            return NodeState.SUCCESS;
        enemy.followingPath = false;
        return NodeState.FAILURE;
    }
}
