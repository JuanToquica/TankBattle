using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;

public class TaskChangeArea : Node
{
    private EnemyAI enemy;

    public TaskChangeArea(EnemyAI enemyScript)
    {
        enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (enemy.changingArea)
        {
            if (enemy.path.status == NavMeshPathStatus.PathComplete)
            {
                if ((enemy.transform.position - enemy.path.corners[enemy.currentCornerInThePath]).magnitude < 1.7f)
                {
                    if (enemy.currentCornerInThePath == enemy.path.corners.Length - 1) //Termino el path a la otra area
                    {
                        enemy.changingArea = false;
                        enemy.currentWaypoint = Random.Range(0, enemy.waypoints.Count - 1);
                        enemy.CalculatePath();
                        return NodeState.SUCCESS;
                    }
                    else
                        enemy.currentCornerInThePath++;
                }
            }
            else
            {
                enemy.ChangeArea();               
            }
            return NodeState.RUNNING;
        }
        return NodeState.FAILURE;     
    }
}
