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
            if ((enemy.transform.position - enemy.corners[enemy.currentCornerInThePath]).magnitude < 3)
            {
                if (enemy.currentCornerInThePath == enemy.corners.Count - 1) //Termino un path a un waypoint
                {                   
                    int random = Random.Range(1, 4);
                    if (random > 2)
                    {
                        enemy.patrolWait = true;
                        enemy.followingPath = false;
                        enemy.path = new NavMeshPath();
                    }
                    else
                    {    
                        int randomWaypoint;
                        do
                        {
                            randomWaypoint = Random.Range(0, enemy.waypoints.Count);
                        } while (enemy.currentWaypoint == randomWaypoint);
                        enemy.currentWaypoint = randomWaypoint;
                        enemy.CalculatePath(enemy.waypoints[enemy.currentWaypoint].position);
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
            int randomWaypoint;
            do
            {
                randomWaypoint = Random.Range(0, enemy.waypoints.Count);
            } while (enemy.currentWaypoint == randomWaypoint);
            enemy.currentWaypoint = randomWaypoint;
            enemy.CalculatePath(enemy.waypoints[enemy.currentWaypoint].position);
            return NodeState.RUNNING;
        }        
        return NodeState.SUCCESS;
    }   
}
