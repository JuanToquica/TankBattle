using UnityEngine;
using BehaviorTree;
using static UnityEngine.GraphicsBuffer;
public class ConditionalHasLineOfSight : Node
{
    private Transform player;
    private Transform turret;
    private EnemyAI enemy;
    
    public ConditionalHasLineOfSight(EnemyAI enemyScript)
    {
        player = enemyScript.player;
        turret = enemyScript.turret;
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float distanceToPlayer = (player.position - turret.position).magnitude;

        RaycastHit hit;
        bool ray = Physics.Raycast(turret.position + turret.up * 0.4f, directionToPlayer, out hit, distanceToPlayer); //Detecta obstaculos entre el enemigo y el player
        Debug.DrawRay(turret.position + turret.up * 0.4f + turret.forward * 2f, directionToPlayer * distanceToPlayer, Color.red);

        if (ray && !hit.transform.CompareTag("Player"))
        {
            enemy.detectingPlayer = false;
            return NodeState.FAILURE;           
        }
        return NodeState.SUCCESS;
    }
}
