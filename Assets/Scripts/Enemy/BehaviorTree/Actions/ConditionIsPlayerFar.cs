using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

public class ConditionIsPlayerFar : Node
{
    private EnemyAI enemy;
    private EnemyManager enemyManager;

    public ConditionIsPlayerFar(EnemyAI enemyScript, EnemyManager enemyManager)
    {
        this.enemy = enemyScript;
        this.enemyManager = enemyManager;
    }

    public override NodeState Evaluate()
    {
        if (enemy.distanceToPlayer > enemy.farDistance)
        {
            if (enemy.enemyArea == 7 && NavMesh.SamplePosition(enemy.player.position, out NavMeshHit hit2, 2f, 1 << 13))
            {
                enemyManager.ChangeAreaToChase(enemy.enemyArea);
            }
            else if (!enemyManager.chasingInArea14 && (enemy.enemyArea == 5 || enemy.enemyArea == 10) && NavMesh.SamplePosition(enemy.player.position, out NavMeshHit hit, 2f, 1 << 14))
            {
                enemyManager.ChangeAreaToChase(enemy.enemyArea);
            }
            
            return NodeState.SUCCESS;
        }
            
        return NodeState.FAILURE;
    }
}
