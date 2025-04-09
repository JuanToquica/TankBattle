using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine;

public class TaskAttack : Node
{
    private EnemyAI _enemyAI;
    public TaskAttack (EnemyAI enemyAI)
    {
        _enemyAI = enemyAI;
    }

    public override NodeState Evaluate()
    {
        if (_enemyAI.CanShoot())
        {
            _enemyAI.Shoot();
            return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }
}
