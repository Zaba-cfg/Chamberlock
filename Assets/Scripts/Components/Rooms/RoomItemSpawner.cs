using System.Collections.Generic;
using UnityEngine;

namespace Components.Rooms
{
    [RequireComponent(typeof(RoomController))]
    public class RoomItemSpawner : MonoBehaviour
    {
        private RoomController _room;
        private RoomSpawnPoint[] _spawnPoints;

        private void Awake()
        {
            _room = GetComponent<RoomController>();
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
            GameObject prefab = _room.Data.ItemPickupPrefab;

            if (prefab == null)
                return;

            if (_spawnPoints.Length == 0)
                throw new MissingReferenceException($"{name}: no RoomSpawnPoint with Kind = Item was found.");

            Instantiate(prefab, _spawnPoints[0].transform.position, Quaternion.identity, transform);
        }

        private RoomSpawnPoint[] CollectSpawnPoints()
        {
            var itemPoints = new List<RoomSpawnPoint>();

            foreach (RoomSpawnPoint point in GetComponentsInChildren<RoomSpawnPoint>(true))
            {
                if (point.Kind == SpawnPointKind.Item)
                    itemPoints.Add(point);
            }

            return itemPoints.ToArray();
        }
    }
}
