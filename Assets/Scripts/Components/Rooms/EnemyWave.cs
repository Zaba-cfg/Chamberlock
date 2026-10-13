using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Rooms
{
    [Serializable]
    public class EnemyWave
    {
        [SerializeField] private List<EnemyWaveEntry> _entries = new();
        [SerializeField, Min(0f)] private float _spawnDelay = 0f;

        public IReadOnlyList<EnemyWaveEntry> Entries => _entries;
        public float SpawnDelay => _spawnDelay;
    }

    [Serializable]
    public class EnemyWaveEntry
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField, Min(1)] private int _count = 1;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int Count => _count;
    }
}