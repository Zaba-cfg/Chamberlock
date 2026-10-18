using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(WeaponHolder))]

    public class EnemyCombat : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _attackRange = 5f;

        public Transform Target => _target;

        public void SetTarget(Transform target)
        {
            if (target == null)
                throw new System.ArgumentNullException(nameof(target));

            _target = target;
        }

        public void SetAttackRange(float attackRange)
        {
            _attackRange = attackRange;
        }

        private WeaponHolder _weaponHolder;

        private void Awake()
        {
            if (_target == null) 
                throw new MissingReferenceException($"{name} is missing a target.");
        
            _weaponHolder = GetComponent<WeaponHolder>();
        }

        public bool CanAttack()
        {
            float distanceSquared = (_target.position - transform.position).sqrMagnitude;

            return distanceSquared <= _attackRange * _attackRange;
        }

        public void Attack(Vector2 direction)
        {
            _weaponHolder.TryFire(direction);
        }
    }
}
