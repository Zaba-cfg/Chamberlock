using System.Collections.Generic;
using UnityEngine;

namespace Components.Rooms
{
    [RequireComponent(typeof(RoomController))]
    public class RoomEnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform _spawnRoot;

        private RoomController _room;
        private RoomSpawnPoint[] _spawnPoints;
        private int _nextSpawnPoint;

        private void Awake()
        {
            _room = GetComponent<RoomController>();

            if (_spawnRoot == null)
                _spawnRoot = transform;

            _spawnPoints = CollectSpawnPoints();
        }

        private void OnEnable()
        {
            _room.RoomActivated += HandleRoomActivated;
        }

        private void OnDisable()
        {
            _room.RoomActivated -= HandleRoomActivated;
        }

        private void HandleRoomActivated()
        {
            foreach (EnemySpawnEntry entry in _room.Data.Enemies)
            {
                if (entry.EnemyPrefab == null)
                    throw new MissingReferenceException($"{name}: an enemy entry has no prefab.");

                for (int i = 0; i < entry.Count; i++)
                    SpawnEnemy(entry.EnemyPrefab);
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            RoomSpawnPoint point = NextSpawnPoint();
            GameObject instance = Instantiate(prefab, point.transform.position, Quaternion.identity, _spawnRoot);

            if (instance.TryGetComponent(out RoomEnemy _))
                return;

            Destroy(instance);
            throw new MissingComponentException($"{name}: enemy prefab '{prefab.name}' is missing a RoomEnemy component.");
        }

        private RoomSpawnPoint NextSpawnPoint()
        {
            if (_spawnPoints.Length == 0)
                throw new MissingReferenceException($"{name}: no RoomSpawnPoint with Kind = Enemy was found.");

            RoomSpawnPoint point = _spawnPoints[_nextSpawnPoint];
            _nextSpawnPoint = (_nextSpawnPoint + 1) % _spawnPoints.Length;
            return point;
        }

        private RoomSpawnPoint[] CollectSpawnPoints()
        {
            var enemyPoints = new List<RoomSpawnPoint>();

            foreach (RoomSpawnPoint point in GetComponentsInChildren<RoomSpawnPoint>(true))
            {
                if (point.Kind == SpawnPointKind.Enemy)
                    enemyPoints.Add(point);
            }

            return enemyPoints.ToArray();
        }
    }
}
