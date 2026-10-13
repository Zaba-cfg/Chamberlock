using Interfaces;
using UnityEngine;

namespace Components.EnemyComponents
{
    public class EnemyAIInput : MonoBehaviour, IMoveInput
    {
        public Vector2 MoveInput { get; private set; }
        public Transform Target { get; private set; }

        private void Awake()
        {
            //if (Target == null) throw new MissingReferenceException($"{name} is missing a target.");
        }

        private void Update()
        {
            if (Target != null)
                MoveInput = Target.position - transform.position;
        }
    
        public void SetTarget(Transform target)
        {
            Target = target;
        }
    }
}