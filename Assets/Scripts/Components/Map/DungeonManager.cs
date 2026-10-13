using System;
using System.Collections;
using System.Collections.Generic;
using Components.Base;
using Components.Rooms;
using Entities.PlayerRelated;
using UI.RoomTransition;
using UnityEngine;

namespace Components.Map
{
    public class DungeonManager : MonoBehaviour
    {
        [SerializeField] private MapData _map;
        [SerializeField] private RoomCamera _camera;
        [SerializeField] private RoomTransitionOverlay _overlay;
        [SerializeField, Min(0f)] private float _playerSpawnDistance = 1f;

        private readonly List<RoomDoor> _subscribedDoors = new();
        private Player _player;
        private Movement _playerMovement;
        private Rigidbody2D _playerBody;
        private Transform _roomsRoot;
        private RoomController _activeRoom;
        private int _activeRoomIndex;
        private bool _isTransitioning;

        public static DungeonManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one DungeonManager is allowed in the scene.");

            Instance = this;

            if (_map == null)
                throw new MissingReferenceException($"{name} is missing MapData.");
            if (_camera == null)
                throw new MissingReferenceException($"{name} is missing RoomCamera.");
            if (_overlay == null)
                throw new MissingReferenceException($"{name} is missing RoomTransitionOverlay.");
            if (_map.Rooms.Count == 0)
                throw new MissingReferenceException($"{name}: {_map.name} has no rooms.");

            ValidateMap();

            _player = FindFirstObjectByType<Player>();

            if (_player == null)
                throw new MissingReferenceException($"{name}: no Player found in the scene.");

            _player.TryGetComponent(out _playerMovement);
            _player.TryGetComponent(out _playerBody);

            _roomsRoot = new GameObject("Rooms").transform;
            _roomsRoot.SetParent(transform);
        }

        private void Start()
        {
            SpawnRoom(0);
            _activeRoom.Activate();
            _camera.Frame(_activeRoom);

            TeleportPlayer(_activeRoom.transform.position);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void ValidateMap()
        {
            for (int i = 0; i < _map.Rooms.Count; i++)
            {
                MapRoomEntry entry = _map.Rooms[i];

                if (entry.RoomPrefab == null)
                    throw new MissingReferenceException($"{name}: {_map.name} entry {i} is missing its room prefab.");

                foreach (RoomDoorLink link in entry.Links)
                {
                    if (link.ToRoomIndex < 0 || link.ToRoomIndex >= _map.Rooms.Count)
                        throw new ArgumentOutOfRangeException(nameof(link.ToRoomIndex),
                            $"{name}: {_map.name} entry {i} links to room {link.ToRoomIndex}, which does not exist.");
                }
            }
        }

        private void HandleDoorEntered(RoomDoor door)
        {
            if (_isTransitioning)
                return;

            MapRoomEntry entry = _map.Rooms[_activeRoomIndex];

            foreach (RoomDoorLink link in entry.Links)
            {
                if (link.From != door.Direction)
                    continue;

                StartCoroutine(TransitionTo(link));
                return;
            }
        }

        private IEnumerator TransitionTo(RoomDoorLink link)
        {
            _isTransitioning = true;
            SetPlayerMovement(false);

            yield return _overlay.FadeOut();

            DestroyActiveRoom();
            SpawnRoom(link.ToRoomIndex);
            PlacePlayerAtDoor(link.To);
            _activeRoom.Activate();
            _camera.Frame(_activeRoom);

            yield return _overlay.FadeIn();

            SetPlayerMovement(true);
            _isTransitioning = false;
        }

        private void SpawnRoom(int index)
        {
            if (index < 0 || index >= _map.Rooms.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"{name}: no room at index {index}.");

            MapRoomEntry entry = _map.Rooms[index];

            if (entry.RoomPrefab == null)
                throw new MissingReferenceException($"{name}: {_map.name} entry {index} is missing its room prefab.");

            GameObject instance = Instantiate(entry.RoomPrefab, (Vector3)entry.WorldPosition, Quaternion.identity, _roomsRoot);

            _activeRoom = instance.GetComponent<RoomController>();

            if (_activeRoom == null)
                throw new MissingReferenceException($"{name}: room '{entry.RoomPrefab.name}' has no RoomController.");

            _activeRoomIndex = index;

            foreach (RoomDoor door in instance.GetComponentsInChildren<RoomDoor>(true))
            {
                door.PlayerEntered += HandleDoorEntered;
                _subscribedDoors.Add(door);
            }
        }

        private void DestroyActiveRoom()
        {
            foreach (RoomDoor door in _subscribedDoors)
            {
                if (door != null)
                    door.PlayerEntered -= HandleDoorEntered;
            }

            _subscribedDoors.Clear();

            if (_activeRoom != null)
                Destroy(_activeRoom.gameObject);

            _activeRoom = null;
        }

        private void PlacePlayerAtDoor(DoorDirection direction)
        {
            RoomDoor door = FindDoor(direction);

            if (door == null)
                throw new MissingReferenceException($"{name}: target room has no {direction} door.");

            Vector2 offset = -DirectionOffset(direction) * _playerSpawnDistance;
            TeleportPlayer(door.transform.position + (Vector3)offset);
        }

        private void TeleportPlayer(Vector3 position)
        {
            _player.transform.position = position;

            if (_playerBody != null)
                _playerBody.position = position;
        }

        private RoomDoor FindDoor(DoorDirection direction)
        {
            foreach (RoomDoor door in _subscribedDoors)
            {
                if (door.Direction == direction)
                    return door;
            }

            return null;
        }

        private void SetPlayerMovement(bool enabled)
        {
            if (_playerBody != null)
                _playerBody.linearVelocity = Vector2.zero;

            if (_playerMovement != null)
                _playerMovement.enabled = enabled;
        }

        private static Vector2 DirectionOffset(DoorDirection direction)
        {
            return direction switch
            {
                DoorDirection.North => Vector2.up,
                DoorDirection.East => Vector2.right,
                DoorDirection.South => Vector2.down,
                DoorDirection.West => Vector2.left,
                _ => Vector2.zero
            };
        }
    }
}