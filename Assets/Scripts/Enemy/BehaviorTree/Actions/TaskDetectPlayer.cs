using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class TaskDetectPlayer : Node
{
    private EnemyAI enemy;
    
    public TaskDetectPlayer(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;        
    }
    public override NodeState Evaluate()
    {
        float detectDistance = enemy.enemyArea == 12 ? enemy.enemyAIParameters.distanceToDetectPlayer * 1.5f : enemy.enemyAIParameters.distanceToDetectPlayer;
        bool detectingByDistance = (Mathf.Abs(enemy.angleToPlayer) <= 90 && enemy.distanceToPlayer < 30) || enemy.distanceToPlayer < detectDistance;

        if (detectingByDistance || enemy.knowsPlayerPosition || NavMesh.SamplePosition(enemy.player.position, out NavMeshHit hit2, 2, 1 <<enemy.enemyArea)) //Verificar distancia o si el jugador se encuentra en el area
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
