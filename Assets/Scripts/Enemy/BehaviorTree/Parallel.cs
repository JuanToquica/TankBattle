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
            int runningCount = 0;

            foreach (var node in children)
            {
                NodeState state = node.Evaluate();
                if (state == NodeState.FAILURE) failureCount++;
                if (state == NodeState.SUCCESS) successCount++;
                if (state == NodeState.RUNNING) runningCount++;
                    
            }
            if (successCount > 0) return NodeState.SUCCESS;
            if (runningCount > 0) return NodeState.RUNNING;
            if (failureCount > 0) return NodeState.FAILURE;

            return NodeState.RUNNING;
        }
    }
}

