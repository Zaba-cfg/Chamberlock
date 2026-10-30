using System;
using System.Collections.Generic;
using Entities.PlayerRelated;
using Systems.Game;
using UnityEngine;

namespace Components.Rooms
{
    public class RoomController : MonoBehaviour
    {
        [SerializeField] private RoomData _data;

        private readonly HashSet<RoomEnemy> _enemies = new();
        private RoomDoor[] _doors;
        private RoomState _state = RoomState.Idle;

        public event Action RoomActivated;
        public event Action RoomCleared;

        public RoomData Data => _data;
        public bool IsActive => _state == RoomState.Active;
        public bool IsCleared => _state == RoomState.Cleared;
        public int EnemiesAlive => _enemies.Count;

        private enum RoomState
        {
            Idle,
            Active,
            Cleared
        }

        private void Awake()
        {
            if (_data == null)
                throw new MissingReferenceException($"{name} is missing RoomData.");

            _doors = GetComponentsInChildren<RoomDoor>(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_state != RoomState.Idle)
                return;

            if (!other.TryGetComponent(out Player _))
                return;

            Activate();
        }

        public void Activate()
        {
            if (_state != RoomState.Idle)
                return;

            _state = RoomState.Active;

            if (RequiresCombat)
                LockDoors();

            RoomActivated?.Invoke();

            if (RequiresCombat)
                ClearIfEmpty();
        }

        public void RegisterEnemy(RoomEnemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            _enemies.Add(enemy);
        }

        public void UnregisterEnemy(RoomEnemy enemy)
        {
            if (enemy == null)
                return;

            _enemies.Remove(enemy);
            ClearIfEmpty();
        }

        public void NotifyEnemyDied(RoomEnemy enemy)
        {
            if (enemy == null)
                return;

            _enemies.Remove(enemy);
            ClearIfEmpty();
        }

        private void ClearIfEmpty()
        {
            if (_state == RoomState.Active && _enemies.Count == 0)
                ClearRoom();
        }

        private bool RequiresCombat =>
            _data.RoomType == RoomType.Combat || _data.RoomType == RoomType.Final;

        private void ClearRoom()
        {
            if (_state == RoomState.Cleared)
                return;

            _state = RoomState.Cleared;
            UnlockDoors();

            if (RunManager.Instance != null)
                RunManager.Instance.AddRoomCleared();

            RoomCleared?.Invoke();

            if (_data.RoomType == RoomType.Final)
                CompleteRun();
        }

        private void CompleteRun()
        {
            if (GameManager.Instance == null)
                return;

            if (RunManager.Instance != null)
                RunManager.Instance.EndRun();

            GameManager.Instance.SetState(GameState.Win);
        }

        private void LockDoors()
        {
            foreach (RoomDoor door in _doors)
                door.Lock();
        }

        private void UnlockDoors()
        {
            foreach (RoomDoor door in _doors)
                door.Unlock();
        }
    }
}