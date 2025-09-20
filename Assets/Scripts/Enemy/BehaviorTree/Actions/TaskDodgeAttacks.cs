using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;
public class TaskDodgeAttacks : Node
{
    private EnemyAI enemy;

    public TaskDodgeAttacks(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        Debug.LogWarning("esquivando");
        if (enemy.path.status == NavMeshPathStatus.PathComplete && enemy.dodgingAttacks)
        {
            if ((enemy.transform.position - enemy.corners[enemy.currentCornerInThePath]).magnitude < 3)
            {
                if (enemy.currentCornerInThePath == enemy.corners.Count - 1)
                {
                    int random = Random.Range(1, 3);
                    if (random > 1)
                    {
                        enemy.patrolWait = true;
                        enemy.followingPath = false;
                        enemy.path = new NavMeshPath();
                    }
                    enemy.dodgingAttacks = false;
                }
                else
                {
                    enemy.currentCornerInThePath++;
                }
            }
        }
        else
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10;
            randomDirection += enemy.transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 5, 1 << enemy.enemyArea))
            {
                float distance = Vector3.Distance(enemy.transform.position, hit.position);
                if (distance >= 5 && distance <= 10)
                {
                    enemy.dodgingAttacks = true;
                    enemy.CalculatePath(hit.position);
                }
            }
            return NodeState.RUNNING;
        }
        return NodeState.SUCCESS;
    }
}
