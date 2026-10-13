using Components.EnemyComponents.EnemyBehaviorRelated;
using Entities.PlayerRelated;
using UnityEngine;

namespace Components.EnemyComponents
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(EnemyBehavior))]
    [RequireComponent(typeof(EnemyCombat))]

    public class EnemyTargetAssigner : MonoBehaviour
    {
        private void Awake()
        {
            Player player = FindFirstObjectByType<Player>();

            if (player == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"{name}: no Player found; skipping automatic target assignment.");
                return;
            }

            EnemyBehavior behavior = GetComponent<EnemyBehavior>();

            if (behavior.Target == null)
                behavior.SetTarget(player.transform);

            EnemyCombat combat = GetComponent<EnemyCombat>();

            if (combat.Target == null)
                combat.SetTarget(player.transform);
        }
    }
}