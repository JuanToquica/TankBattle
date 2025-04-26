using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class TaskPatrol : Node
{
    private EnemyAI enemy;    

    public TaskPatrol(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.path.status == NavMeshPathStatus.PathComplete)
        {           
            if ((enemy.transform.position - enemy.path.corners[enemy.currentCornerInThePath]).magnitude < 1.7f)
            {
                if (enemy.currentCornerInThePath == enemy.path.corners.Length - 1) //Termino un path a un waypoint
                {
                    if (enemy.currentWaypoint == enemy.waypoints.Length - 1)
                        enemy.currentWaypoint = 0;
                    else
                        enemy.currentWaypoint++;
                    enemy.CalculatePath();
                }
                else
                {
                    if (enemy.currentCornerInThePath == enemy.path.corners.Length - 1)
                        enemy.currentCornerInThePath = 1;
                    else
                        enemy.currentCornerInThePath++;
                }    
            }
        }
        else
        {
            enemy.CalculatePath();
            return NodeState.RUNNING;
        }
        enemy.followingPath = true;
        return NodeState.SUCCESS;
    }
}
