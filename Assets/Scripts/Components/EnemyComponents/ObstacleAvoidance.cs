using UnityEngine;

namespace Components.EnemyComponents
{
    public class ObstacleAvoidance : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _checkDistance = 1.5f;
        [SerializeField, Range(15f, 90f)] private float _whiskerAngle = 45f;

        private int _preferredSide = 1;

        public Vector2 AdjustDirection(Vector2 desired)
        {
            if (desired == Vector2.zero)
                return desired;

            if (ObstacleRaycast.IsPathClear(transform.position, desired, _checkDistance))
            {
                _preferredSide = 1;
                return desired;
            }

            Vector2 leftDirection = Rotate(desired, _whiskerAngle);
            Vector2 rightDirection = Rotate(desired, -_whiskerAngle);

            bool leftClear = ObstacleRaycast.IsPathClear(transform.position, leftDirection, _checkDistance);
            bool rightClear = ObstacleRaycast.IsPathClear(transform.position, rightDirection, _checkDistance);

            if (leftClear && rightClear)
                return Rotate(desired, _preferredSide * _whiskerAngle);

            if (leftClear)
            {
                _preferredSide = 1;
                return leftDirection;
            }

            if (rightClear)
            {
                _preferredSide = -1;
                return rightDirection;
            }

            return Rotate(desired, _preferredSide * 90f);
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            return (Vector2)(Quaternion.Euler(0f, 0f, degrees) * direction);
        }
    }
}
