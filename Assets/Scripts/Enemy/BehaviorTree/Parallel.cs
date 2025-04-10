using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Parallel : Node
    {
        private List<Node> children;
        public Parallel(List<Node> nodes)
        {
            children = new List<Node>(nodes);
        }

        public override NodeState Evaluate()
        {
            int successCount = 0;
            int failureCount = 0;

            foreach (var node in children)
            {
                NodeState state = node.Evaluate();
                if (state == NodeState.FAILURE) failureCount++;
                if (state == NodeState.SUCCESS) successCount++;
                    
            }
            if (successCount >= 1) return NodeState.SUCCESS;
            if (failureCount > 0) return NodeState.FAILURE;

            return NodeState.RUNNING;
        }
    }
}

