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
        
        public GameObject ChooseRandom()
        {
            List<GameObject> validOptions = new();

            foreach (GameObject option in _room.Data.ItemPickupPrefab)
            {
                if (option != null)
                    validOptions.Add(option);
            }

            if (validOptions.Count == 0)
            {
                UnityEngine.Debug.LogWarning("No hay prefabs disponibles para elegir.", this);
                return null;
            }

            int randomIndex = Random.Range(0, validOptions.Count);

            return validOptions[randomIndex];
        }

        private void HandleRoomActivated()
        {
            if (_spawnPoints.Length == 0)
                throw new MissingReferenceException(
                    $"{name}: no RoomSpawnPoint with Kind = Item was found.");

            GameObject prefab = ChooseRandom();

            if (prefab == null)
                return;

            Instantiate(
                prefab,
                _spawnPoints[0].transform.position,
                Quaternion.identity,
                transform
            );
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
