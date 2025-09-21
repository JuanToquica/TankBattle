using BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

public class TaskPausePatrol : Node
{   
    private EnemyAI enemy;
    private PlayerAttack playerAttack;
    private float waitTime;
    private float timer;

    public TaskPausePatrol(EnemyAI enemy, PlayerAttack playerAttack)
    {
        this.enemy = enemy;
        this.playerAttack = playerAttack;
    }

    public override NodeState Evaluate()
    {
        if (enemy.detectingPlayer &&  Time.time - playerAttack.lastShot > enemy.enemyAIParameters.playerIdleTimeThreshold) //Si el jugador ha dejado de atacar
        {
            enemy.DoPausePatrol();
        }         

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
                if (enemy.knowsPlayerPosition)
                    waitTime = Random.Range(1f, 2f);
                else
                    waitTime = Random.Range(1f, 5f);
            }
            return NodeState.RUNNING;
        }
        else
        {
            return NodeState.FAILURE;
        }
    }
}
