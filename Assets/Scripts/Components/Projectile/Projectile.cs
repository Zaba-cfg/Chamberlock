using System;
using Components.Base;
using Components.Base.HealthRelated;
using Interfaces;
using Systems.Pool;
using UnityEngine;

namespace Components.Projectile
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]

    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 20f;

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        private float _damage;
        private GameObject _owner;
        private bool _ownerIsEnemy;
        private float _returnTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (Time.time < _returnTime)
                return;

            ReturnToPool();
        }

        public void Initialize(Vector2 direction, float damage, GameObject owner)
        {
            if (direction == Vector2.zero)
                throw new ArgumentException($"{name} cannot have a zero direction.");

            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            _damage = damage;
            _owner = owner;
            _ownerIsEnemy = owner.GetComponentInParent<IEnemy>() != null;

            _rigidbody.linearVelocity = direction.normalized * _speed;

            _returnTime = Time.time + 5f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_owner != null
                && (other.gameObject == _owner || other.transform.IsChildOf(_owner.transform)))
                return;

            if (other.isTrigger)
                return;

            if (other.TryGetComponent(out Health health))
            {
                if (_ownerIsEnemy && other.GetComponentInParent<IEnemy>() != null)
                    return;

                health.TakeDamage(_damage);
            }

            ReturnToPool();
        }

        public void OnGetFromPool()
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }

        public void OnReturnToPool()
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }

        private void ReturnToPool()
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.Release(this);
            else
                Destroy(gameObject);
        }
    }
}