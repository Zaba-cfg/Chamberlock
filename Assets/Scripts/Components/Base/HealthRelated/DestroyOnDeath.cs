using Components.Weapons;
using UnityEngine;

namespace Components.Base.HealthRelated
{
    [RequireComponent(typeof(Health))]
    public class DestroyOnDeath : MonoBehaviour
    {
        private Health _health;
        private WeaponHolder _holder;

        private void Awake()
        {
            _health = GetComponent<Health>();

            if (TryGetComponent(out WeaponHolder holder))
            {
                _holder = holder;
            }
        }

        private void OnEnable()
        {
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Died -= HandleDied;
        }

        private void HandleDied()
        {
            if (_holder != null)
            {
                _holder.DropCurrentWeapon();
            }
            
            Destroy(gameObject);
        }
    }
}
