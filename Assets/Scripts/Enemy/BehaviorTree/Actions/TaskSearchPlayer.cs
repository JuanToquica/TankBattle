using BehaviorTree;
using UnityEngine;

public class TaskSearchPlayer : Node
{
    private EnemyAI enemy;

    public TaskSearchPlayer(EnemyAI enemyScript)
    {
        this.enemy = enemyScript;
    }

    public override NodeState Evaluate()
    {
        return NodeState.SUCCESS;
    }
}
