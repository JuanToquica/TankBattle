using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Node
{
    private Node child;
    public Inverter(Node node)
    {
        child = node;
    }
    public override NodeState Evaluate()
    {
        NodeState state = child.Evaluate();

        if (state == NodeState.FAILURE)
            return NodeState.SUCCESS;
        if (state == NodeState.SUCCESS)
            return NodeState.FAILURE;
        return NodeState.RUNNING;
    }
}
