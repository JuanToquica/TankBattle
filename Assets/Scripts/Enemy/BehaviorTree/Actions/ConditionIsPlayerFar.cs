using BehaviorTree;
using UnityEngine;

public class ConditionIsPlayerFar : Node
{
    private EnemyAI enemy;
    private Transform player;
    private Transform turret;

    public ConditionIsPlayerFar(EnemyAI enemyScript)
    {
        player = enemyScript.player;
        turret = enemyScript.turret;
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        float distanceToPlayer = (player.position - turret.position).magnitude;
        if (distanceToPlayer > enemy.stoppingDistance)
            return NodeState.SUCCESS;
        return NodeState.FAILURE;
    }
}
