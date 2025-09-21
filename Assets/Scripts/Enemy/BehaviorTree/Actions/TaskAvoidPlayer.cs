using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

public class TaskAvoidPlayer : Node
{
    private EnemyAI enemy;

    public TaskAvoidPlayer(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        int safeNearestPoint = 0;
        float safeNearestDistance = 100;

        for (int i = 0; i < enemy.waypoints.Count; i++)
        {
            float distancePointToPlayer = (enemy.waypoints[i].position - enemy.player.position).magnitude;
            Vector3 directionToPoint = (enemy.waypoints[i].position - enemy.transform.position).normalized;
            if (distancePointToPlayer < safeNearestDistance && distancePointToPlayer > enemy.enemyAIParameters.nearDistance && Vector3.Dot(directionToPoint, enemy.directionToPlayer) < 0)
            {
                safeNearestDistance = distancePointToPlayer;
                safeNearestPoint = i;
            }
        }

        if (enemy.path.status == NavMeshPathStatus.PathComplete && enemy.currentWaypoint == safeNearestPoint)
        {
            if ((enemy.transform.position - enemy.corners[enemy.currentCornerInThePath]).magnitude < 3)
            {
                if (enemy.currentCornerInThePath == enemy.corners.Count - 1)
                {
                    Debug.Log("Alcanzado");
                    enemy.followingPath = false;
                }
                else
                {
                    enemy.currentCornerInThePath++;
                }
            }
        }
        else
        {
            enemy.currentWaypoint = safeNearestPoint;
            enemy.CalculatePath(enemy.waypoints[enemy.currentWaypoint].position);
            return NodeState.RUNNING;
        }
        return NodeState.SUCCESS;
    }
}
