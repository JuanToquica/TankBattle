using UnityEngine;
using BehaviorTree;


public class TaskDetectPlayer : Node
{
    private EnemyAI enemy;
    private Transform player;
    private Transform turret;
    
    public TaskDetectPlayer(EnemyAI enemyScript)
    {
        player = enemyScript.player;
        turret = enemyScript.turret;
        this.enemy = enemyScript;        
    }
    public override NodeState Evaluate()
    {             
        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float angle = Vector3.Angle(turret.forward, directionToPlayer);
        float distanceToPlayer = (player.position - turret.position).magnitude;

        if (distanceToPlayer < enemy.distanceToDetectPlayer)
        {
            enemy.detectingPlayer = true;
        }           
        else if (angle <= 120)
        {
            RaycastHit hit;
            bool ray = Physics.Raycast(turret.position + turret.up * 0.4f, directionToPlayer, out hit, distanceToPlayer); //Detecta obstaculos entre el enemigo y el player
            if (ray && hit.transform.CompareTag("Player")) enemy.detectingPlayer = true;
        }
        else
        {
            enemy.detectingPlayer = false;
        }
            
        if (enemy.detectingPlayer || enemy.knowsPlayerPosition) return NodeState.SUCCESS;      

        return NodeState.FAILURE;
    }
}
