using UnityEngine;
using BehaviorTree;
using UnityEngine.Rendering.Universal;

public class TaskAim : Node
{
    private EnemyAI enemy;

    public TaskAim(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;      
    }

    private float CalculateTolerance()
    {
        if (enemy.distanceToPlayer > 25)
            return 1f;
            
        float tolerance = -0.28f * enemy.distanceToPlayer + enemy.maxAimingTolerance; // -0.28 es la pendiente de la interpolacion
        return tolerance;
    }
       
    public override NodeState Evaluate()
    {
        if (Mathf.Abs(enemy.angleToPlayer) < enemy.maxAimingTolerance)
        {
            if (Mathf.Abs(enemy.angleToPlayer) > CalculateTolerance())
                enemy.RotateTurret();
            return NodeState.SUCCESS;
        }           
        else
        {
            enemy.RotateTurret();
            return NodeState.RUNNING;
        }           
    }
}
