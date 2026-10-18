using Components.Base;
using Components.Base.HealthRelated;
using UnityEngine;

namespace Components.EnemyComponents
{
    [DefaultExecutionOrder(-90)]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(EnemyCombat))]
    [RequireComponent(typeof(SpriteRenderer))]

    public class EnemyDataApplier : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;

        public EnemyData Data => _data;

        private void Awake()
        {
            if (_data == null)
                throw new MissingReferenceException($"{name} is missing EnemyData.");

            GetComponent<Health>().SetMaxHealth(_data.MaxHealth);
            GetComponent<Movement>().SetBaseSpeed(_data.MoveSpeed);
            GetComponent<EnemyCombat>().SetAttackRange(_data.AttackRange);

            if (_data.Sprite != null)
                GetComponent<SpriteRenderer>().sprite = _data.Sprite;
        }
    }
}
