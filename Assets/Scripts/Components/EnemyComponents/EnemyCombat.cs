using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(WeaponHolder))]

    public class EnemyCombat : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _attackRange = 5f;
        [SerializeField] private bool _requireLineOfSight = true;

        public Transform Target => _target;
        public float AttackRange => _attackRange;

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
            Vector2 toTarget = _target.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance > _attackRange)
                return false;

            if (_requireLineOfSight && distance > 0f && !ObstacleRaycast.IsPathClear(transform.position, toTarget / distance, distance))
                return false;

            return true;
        }

        public void Attack(Vector2 direction)
        {
            _weaponHolder.TryFire(direction);
        }
    }
}
