using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class Sequence : Node
    {
        private List<Node> children;
        public Sequence(List<Node> nodes)
        {
            children = new List<Node>(nodes);
        }

        public override NodeState Evaluate()
        {
            bool anyChildRunning = false;

            foreach (var node in children)
            {
                NodeState state = node.Evaluate();
                if (state == NodeState.FAILURE)
                    return NodeState.FAILURE;
                if (state == NodeState.RUNNING)
                    anyChildRunning = true;
            }
            return anyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        }
    }
}

