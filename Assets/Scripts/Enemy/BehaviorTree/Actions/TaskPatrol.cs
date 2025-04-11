using BehaviorTree;
using UnityEngine;

public class TaskPatrol : Node
{

    public override NodeState Evaluate()
    {
        Debug.Log("Patrullando");
        return NodeState.SUCCESS;
    }
}
