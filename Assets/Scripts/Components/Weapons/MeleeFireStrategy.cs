using System;
using System.Collections.Generic;
using Components.Base;
using Components.Base.HealthRelated;
using Interfaces;
using UnityEngine;

namespace Components.Weapons
{
    [CreateAssetMenu(fileName = "Melee Fire Strategy", menuName = "Weapons/Fire Strategies/Melee")]
    public class MeleeFireStrategy : WeaponFireStrategy
    {
        [SerializeField, Min(0f)] private float _range = 1.25f;
        [SerializeField, Min(0f)] private float _radius = 0.5f;

        [SerializeField, Range(0.5f, 1f)] private float _minimumRadiusMultiplier = 0.7f;

        public float Range => _range;
        public float Radius => _radius;
        public float MinimumRadiusMultiplier => _minimumRadiusMultiplier;

        public override void Execute(WeaponFireContext context)
        {
            if (context.Weapon == null)
                throw new ArgumentNullException(nameof(context.Weapon));

            if (context.Direction == Vector2.zero)
                return;

            float angleStep = context.AttackCount > 1
                ? context.SpreadAngle / (context.AttackCount - 1)
                : 0f;

            float startAngle = -context.SpreadAngle / 2f;

            float hitRadius = Mathf.Max(
                _radius / Mathf.Sqrt(context.AttackCount),
                _radius * _minimumRadiusMultiplier);

            HashSet<Health> damagedTargets = new();

            GameObject owner = context.Weapon.Owner;
            bool ownerIsEnemy = owner.GetComponentInParent<IEnemy>() != null;

            for (int i = 0; i < context.AttackCount; i++)
            {
                float angle = startAngle + angleStep * i;

                Vector2 attackDirection =
                    Quaternion.Euler(0f, 0f, angle) *
                    context.Direction;

                Vector2 hitPosition =
                    (Vector2)context.Weapon.Muzzle.position +
                    attackDirection.normalized * _range;

                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    hitPosition,
                    hitRadius);

                foreach (Collider2D hit in hits)
                {
                    if (hit.isTrigger)
                        continue;

                    if (hit.gameObject == owner || hit.transform.IsChildOf(owner.transform))
                        continue;

                    if (!hit.TryGetComponent(out Health health))
                        continue;

                    if (ownerIsEnemy && hit.GetComponentInParent<IEnemy>() != null)
                        continue;

                    if (!damagedTargets.Add(health))
                        continue;

                    health.TakeDamage(context.Weapon.Damage);
                }
            }
        }
    }
}