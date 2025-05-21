using BehaviorTree;
using UnityEngine;

public class TaskPausePatrol : Node
{   
    private EnemyAI enemy;
    private float waitTime;
    private float timer;

    public TaskPausePatrol(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public override NodeState Evaluate()
    {
        if (enemy.patrolWait)
        {
            if (waitTime > 0)
            {
                timer += Time.deltaTime;
                if (timer >= waitTime)
                {
                    enemy.patrolWait = false;
                    timer = 0f;
                    waitTime = 0;
                    return NodeState.SUCCESS;
                }
            }
            else
            {
                waitTime = Random.Range(1,5);
            }           
            return NodeState.RUNNING;
        }
        else
        {
            return NodeState.FAILURE;
        }
    }
}
