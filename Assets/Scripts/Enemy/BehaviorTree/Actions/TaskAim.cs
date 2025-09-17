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
       
    public override NodeState Evaluate()
    {
        if (Mathf.Abs(enemy.angleToPlayer) < enemy.enemyAIParameters.maxAimingTolerance)
        {   
            if (Mathf.Abs(enemy.angleToPlayer) > 2)
            {
                if (enemy.distanceToPlayer > 30)
                {
                    enemy.RotateTurret(enemy.turretRotationSpeed);
                }
                else
                {
                    float t = Mathf.InverseLerp(0f, 30, enemy.distanceToPlayer);
                    float calculatedRotationSpeed = Mathf.Lerp(20, enemy.turretRotationSpeed, t);
                    enemy.RotateTurret(calculatedRotationSpeed);
                }
            }
            return NodeState.SUCCESS;
        }           
        else
        {
            enemy.RotateTurret(enemy.turretRotationSpeed);
            return NodeState.RUNNING;
        }           
    }
}
