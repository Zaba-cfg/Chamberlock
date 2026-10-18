using Interfaces;
using UnityEngine;

namespace Components.EnemyComponents
{
    public class EnemyAIInput : MonoBehaviour, IMoveInput
    {
        public Vector2 MoveInput { get; private set; }
        public Transform Target { get; private set; }

        [SerializeField, Range(0.1f, 1f)] private float _retreatSpeedMultiplier = 1f;

        private bool _retreating;
        private bool _moveEnabled = true;

        private void Awake()
        {
            //if (Target == null) throw new MissingReferenceException($"{name} is missing a target.");
        }

        private void Update()
        {
            if (Target == null)
                return;

            if (!_moveEnabled)
            {
                MoveInput = Vector2.zero;
                return;
            }

            Vector2 direction = Target.position - transform.position;

            if (_retreating)
                direction = -direction * _retreatSpeedMultiplier;

            MoveInput = direction;
        }
    
        public void SetTarget(Transform target)
        {
            Target = target;
        }

        public void SetRetreating(bool retreating)
        {
            _retreating = retreating;
        }

        public void SetMoveEnabled(bool enabled)
        {
            _moveEnabled = enabled;
        }
    }
}