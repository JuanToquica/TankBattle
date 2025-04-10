using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine;

public class TaskAttack : Node
{
    private EnemyAI enemyAI;
    public TaskAttack (EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public override NodeState Evaluate()
    {
        if (enemyAI.CanShoot())
        {
            enemyAI.Shoot();
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }
}
