using BehaviorTree;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class TaskChasePlayer : Node
{
    private EnemyAI enemy;

    public TaskChasePlayer(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        Debug.LogWarning("persiguiendo");
        int nearestPoint = 0;
        float nearestDistance = 100;

        for (int i = 0; i < enemy.waypoints.Count; i++)
        {
            float distanceToPlayer = (enemy.waypoints[i].position - enemy.player.position).magnitude;
            if (distanceToPlayer < nearestDistance)
            {
                nearestDistance = distanceToPlayer;
                nearestPoint = i;
            }
                
        }

        if (enemy.path.status == NavMeshPathStatus.PathComplete && enemy.currentWaypoint == nearestPoint)
        {
            if ((enemy.transform.position - enemy.corners[enemy.currentCornerInThePath]).magnitude < 3)
            {
                if (enemy.currentCornerInThePath == enemy.corners.Count - 1)
                {
                    enemy.followingPath = false;
                    enemy.path = new NavMeshPath();
                }
                else
                {
                    enemy.currentCornerInThePath++;
                }
            }
        }
        else
        {
            enemy.currentWaypoint = nearestPoint;
            enemy.CalculatePath(enemy.waypoints[enemy.currentWaypoint].position);
            return NodeState.RUNNING;
        }
        return NodeState.SUCCESS;
    }
}
