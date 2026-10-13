using System;
using UnityEngine;

namespace Components.Rooms
{
    [Serializable]
    public class EnemySpawnEntry
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField, Min(1)] private int _count = 1;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int Count => _count;
    }
}
