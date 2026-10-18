using Components.Base;
using Components.Base.HealthRelated;
using Components.Base.Weapon;
using Components.EnemyComponents;
using Components.EnemyComponents.EnemyBehaviorRelated;
using Components.Weapons;
using Interfaces;
using UnityEngine;

namespace Entities.EnemyRelated
{
    [RequireComponent(typeof(EnemyAIInput))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(LookDirection))]
    [RequireComponent(typeof(WeaponHolder))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyCombat))]
    [RequireComponent(typeof(WeaponDetection))]
    [RequireComponent(typeof(EnemyWeaponDecision))]
    [RequireComponent(typeof(MeleeBehavior))]

    public class MeleeEnemy : MonoBehaviour, IEnemy
    {
    }
}
