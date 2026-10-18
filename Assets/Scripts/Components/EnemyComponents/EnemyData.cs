using UnityEngine;

namespace Components.EnemyComponents
{
    [CreateAssetMenu(fileName = "Enemy Data", menuName = "Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private string _enemyName;
        [SerializeField] private Sprite _sprite;
        [SerializeField, Min(1f)] private float _maxHealth = 50f;
        [SerializeField, Min(0f)] private float _moveSpeed = 2f;
        [SerializeField, Min(0f)] private float _attackRange = 1.5f;

        public string EnemyName => _enemyName;
        public Sprite Sprite => _sprite;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
    }
}
