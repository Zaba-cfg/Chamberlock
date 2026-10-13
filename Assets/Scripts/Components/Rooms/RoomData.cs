using System.Collections.Generic;
using UnityEngine;

namespace Components.Rooms
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Rooms/RoomData")]
    public class RoomData : ScriptableObject
    {
        [SerializeField] private RoomType _roomType;
        [SerializeField] private Vector2Int _dimensions = new(20, 11);
        [SerializeField] private List<EnemyWave> _enemyWaves = new();

        public RoomType RoomType => _roomType;
        public Vector2Int Dimensions => _dimensions;
        public IReadOnlyList<EnemyWave> EnemyWaves => _enemyWaves;
    }
}