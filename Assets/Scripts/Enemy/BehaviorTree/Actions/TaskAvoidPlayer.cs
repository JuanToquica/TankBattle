using BehaviorTree;
using UnityEngine;

public class TaskAvoidPlayer : Node
{
    private EnemyAI enemy;

    public TaskAvoidPlayer(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        return NodeState.SUCCESS;
    }
}
