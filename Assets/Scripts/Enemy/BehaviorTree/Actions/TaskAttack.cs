using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine;

public class TaskAttack : Node
{
    private EnemyAttack enemyAttack;
    public TaskAttack (EnemyAttack enemyAttack)
    {
        this.enemyAttack = enemyAttack;
    }

    public override NodeState Evaluate()
    {
        if (enemyAttack.CanShoot())
        {
            enemyAttack.Shoot();
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }
}
