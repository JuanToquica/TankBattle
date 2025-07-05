using UnityEngine;
using BehaviorTree;

public class TaskDetectPlayer : Node
{
    private EnemyAI enemy;
    
    public TaskDetectPlayer(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;        
    }
    public override NodeState Evaluate()
    {
        float detectDistance = enemy.enemyArea == 12 ? enemy.distanceToDetectPlayer * 1.5f : enemy.distanceToDetectPlayer;
        if ((Mathf.Abs(enemy.angleToPlayer) <= 90 && enemy.distanceToPlayer < 30) || enemy.distanceToPlayer < detectDistance || enemy.knowsPlayerPosition)
        {
            RaycastHit hit;
            bool ray = Physics.Raycast(enemy.turret.position + enemy.turret.up * 0.4f, enemy.directionToPlayer, out hit, enemy.distanceToPlayer); //Detecta obstaculos entre el enemigo y el player
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
