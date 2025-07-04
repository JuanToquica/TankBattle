using BehaviorTree;
using UnityEngine;
using UnityEngine.Rendering;

public class TaskWatch : Node
{
    private EnemyAI enemy;
    private Vector3 randomPoint;
    private bool waiting;
    private float waitTime;
    private float timer;

    public TaskWatch(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        if (waiting)
        {
            if (waitTime > 0)
            {
                timer += Time.deltaTime;
                if (timer >= waitTime)
                {
                    waiting = false;
                    timer = 0f;
                    waitTime = 0;
                    return NodeState.SUCCESS;
                }
            }
            else
            {
                waitTime = Random.Range(1f, 2f);
            }
            return NodeState.RUNNING;
        }
        else
        {
            if (randomPoint != Vector3.zero)
            {
                Vector3 flatForward = enemy.turret.forward;
                flatForward.y = 0;
                Vector3 directionToPoint = (randomPoint - enemy.turret.position).normalized;
                float angleToRandomPoint = Vector3.SignedAngle(flatForward, directionToPoint, Vector3.up);
                if (Mathf.Abs(angleToRandomPoint) < enemy.maxAimingTolerance)
                {
                    int random = Random.Range(1, 3);
                    if (random == 1)
                        waiting = true;
                    randomPoint = Vector3.zero;
                    return NodeState.SUCCESS;
                }
                else
                {
                    enemy.RotatoTurretToWatch(angleToRandomPoint);
                    return NodeState.RUNNING;
                }
            }
            else
            {
                if (enemy.knowsPlayerPosition)
                {
                    Vector3 perpendicularDirection = Vector3.Cross(enemy.directionToPlayer, Vector3.up);
                    float randomDistance = Random.Range(-60f, 60f);
                    randomPoint = enemy.transform.position + (enemy.directionToPlayer * enemy.distanceToPlayer) + (perpendicularDirection * randomDistance);
                }
                else
                {
                    randomPoint = new Vector3(Random.Range(-60f, 60f), 0, enemy.transform.position.z - 60);
                }               
                return NodeState.RUNNING;
            }
        }        
    }
}
