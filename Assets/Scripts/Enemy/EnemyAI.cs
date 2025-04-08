using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTree
{
    public class EnemyAI : MonoBehaviour
    {
        private Node _root = null;

        private void Start()
        {
            _root = SetUpTree();
        }
        private void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        private Node SetUpTree()
        {
            return _root;
        }
    }
}
