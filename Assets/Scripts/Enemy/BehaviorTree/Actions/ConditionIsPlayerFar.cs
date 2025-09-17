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
        if (enemy.distanceToPlayer > enemy.enemyAIParameters.farDistance)
        {
            if (enemy.enemyArea == 7 || (!enemyManager.chasingInArea14 && (enemy.enemyArea == 5 || enemy.enemyArea == 10)))
                enemy.EvaluateAreaChange();
            if (enemy.enemyArea == 13 || (enemyManager.chasingInArea14 && enemy.enemyArea == 14))
                enemy.EvaluateBackToOriginalArea();

            return NodeState.SUCCESS;
        }
            
        return NodeState.FAILURE;
    }
}
