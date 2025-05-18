using UnityEngine;
using BehaviorTree;
public class TaskDodgeAttacks : Node
{
    private EnemyAI enemy;

    public TaskDodgeAttacks(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        return NodeState.SUCCESS;
    }
}
