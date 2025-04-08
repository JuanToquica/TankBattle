using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Selector : Node
    {
        private List<Node> children;
        public Selector(List<Node> nodes)
        {
            children = new List<Node>(nodes);
        }

        public override NodeState Evaluate()
        {
            foreach (var node in children)
            {
                NodeState state = node.Evaluate();
                if (state == NodeState.SUCCESS || state == NodeState.RUNNING)
                    return state;
            }
            return NodeState.FAILURE;
        }
    }
}
