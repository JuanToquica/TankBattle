using BehaviorTree;
using UnityEngine;

public class TaskChasePlayer : Node
{


    public override NodeState Evaluate()
    {
        Debug.Log("Persiguiendo jugador");
        return NodeState.SUCCESS;
    }
}
