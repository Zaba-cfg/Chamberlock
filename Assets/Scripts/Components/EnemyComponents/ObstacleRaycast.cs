using Components.Base.HealthRelated;
using UnityEngine;

namespace Components.EnemyComponents
{
    public static class ObstacleRaycast
    {
        private static readonly RaycastHit2D[] Buffer = new RaycastHit2D[16];

        public static bool IsPathClear(Vector2 origin, Vector2 direction, float distance)
        {
            if (direction == Vector2.zero || distance <= 0f)
                return true;

            int count = Physics2D.RaycastNonAlloc(origin, direction, Buffer, distance);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = Buffer[i].collider;

                if (hit.isTrigger)
                    continue;

                if (hit.GetComponentInParent<Health>() != null)
                    continue;

                return false;
            }

            return true;
        }
    }
}
