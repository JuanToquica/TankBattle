using UnityEngine;
using BehaviorTree;
using static UnityEngine.GraphicsBuffer;
public class ConditionalHasLineOfSight : Node
{
    private EnemyAI enemy;
    
    public ConditionalHasLineOfSight(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        RaycastHit hit;
        bool ray = Physics.Raycast(enemy.turret.position + enemy.turret.up * 0.4f, enemy.directionToPlayer, out hit, enemy.distanceToPlayer); //Detecta obstaculos entre el enemigo y el player
        Debug.DrawRay(enemy.turret.position + enemy.turret.up * 0.4f + enemy.turret.forward * 2f, enemy.directionToPlayer * enemy.distanceToPlayer, Color.red);

        if (ray && !hit.transform.CompareTag("Player"))
        {
            enemy.detectingPlayer = false;
            return NodeState.FAILURE;           
        }
        return NodeState.SUCCESS;
    }
}
