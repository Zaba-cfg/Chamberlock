using Components.Modifiers;
using Interfaces;
using UnityEngine;

namespace Components.Base
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ModifierInventory))]

    public class Movement : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private IMoveInput _moveInput;
        private ModifierInventory _modifierInventory;
    
        [SerializeField] private float _baseSpeed = 5f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _moveInput = GetComponent<IMoveInput>();
            _modifierInventory = GetComponent<ModifierInventory>();
        
            if (_moveInput == null)
                throw new MissingComponentException($"{name} requires a component implementing IMoveInput.");
        }

        public void SetBaseSpeed(float baseSpeed)
        {
            _baseSpeed = baseSpeed;
        }

        private void FixedUpdate()
        {
            var inputValue = Vector2.ClampMagnitude(_moveInput.MoveInput,1f);
        
            float speed = _modifierInventory.CalculateValue(_baseSpeed, ModifierStat.MovementSpeed);
    
            _rigidbody.linearVelocity = inputValue * speed;
        }
    }
}
