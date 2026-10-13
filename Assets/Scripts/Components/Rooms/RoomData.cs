using System.Collections.Generic;
using UnityEngine;

namespace Components.Rooms
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Rooms/RoomData")]
    public class RoomData : ScriptableObject
    {
        [SerializeField] private RoomType _roomType;
        [SerializeField] private Vector2Int _dimensions = new(20, 11);
        [SerializeField] private List<EnemySpawnEntry> _enemies = new();
        [SerializeField] private GameObject _itemPickupPrefab;

        public RoomType RoomType => _roomType;
        public Vector2Int Dimensions => _dimensions;
        public IReadOnlyList<EnemySpawnEntry> Enemies => _enemies;
        public GameObject ItemPickupPrefab => _itemPickupPrefab;
    }
}