using BehaviorTree;
using UnityEngine;

public class TaskWatch : Node
{
    private EnemyAI enemy;

    public TaskWatch(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        return NodeState.SUCCESS;
    }
}
