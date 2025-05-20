using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class TaskPatrol : Node
{
    private EnemyAI enemy;    
    private Coroutine coroutine;

    public TaskPatrol(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.path.status == NavMeshPathStatus.PathComplete)
        {           
            if ((enemy.transform.position - enemy.corners[enemy.currentCornerInThePath]).magnitude < 4f)
            {
                if (enemy.currentCornerInThePath == enemy.corners.Count - 1) //Termino un path a un waypoint
                {
                    int random = Random.Range(1, 3);
                    if (random > 1)
                    {
                        enemy.patrolWait = true;
                        enemy.followingPath = false;
                        enemy.path = new NavMeshPath();
                    }
                    else
                    {
                        enemy.currentWaypoint = Random.Range(0, enemy.waypoints.Count);
                        enemy.CalculatePath();
                    }   
                }
                else
                {
                    enemy.currentCornerInThePath++;
                }    
            }
        }
        else
        {
            enemy.CalculatePath();
            return NodeState.RUNNING;
        }        
        return NodeState.SUCCESS;
    }   
}
