using UnityEngine;
using BehaviorTree;
using UnityEngine.Rendering.Universal;

public class TaskAim : Node
{
    private EnemyAI enemy;
    private Transform player;
    private Transform turret;

    public TaskAim(EnemyAI enemyScript)
    {
        player = enemyScript.player;
        turret = enemyScript.turret;
        this.enemy = enemyScript;      
    }


    private void RotateTurret(float angle)
    {
        turret.Rotate(0, enemy.turretRotationSpeed * Mathf.Sign(angle) * Time.fixedDeltaTime, 0);
    }

    private float CalculateTolerance()
    {
        float distanceToPlayer = (player.position - turret.position).magnitude;
        if (distanceToPlayer > 25)
            return 1f;
            
        float tolerance = -0.28f * distanceToPlayer + enemy.maxAimingTolerance; // -0.28 es la pendiente de la interpolacion
        return tolerance;
    }
    
        
    public override NodeState Evaluate()
    {
        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float angle = Vector3.SignedAngle(turret.forward, directionToPlayer, Vector3.up);       

        if (Mathf.Abs(angle) < enemy.maxAimingTolerance)
        {
            if (Mathf.Abs(angle) > CalculateTolerance())
                RotateTurret(angle);
            return NodeState.SUCCESS;
        }           
        else
        {
            RotateTurret(angle);
            return NodeState.FAILURE;
        }           
    }
}
