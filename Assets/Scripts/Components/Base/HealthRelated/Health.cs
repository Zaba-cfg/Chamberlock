using System;
using UnityEngine;

namespace Components.Base.HealthRelated
{
    public class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float _maxHealth;
    
        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;
        public bool IsDead => CurrentHealth <= 0f;
    
        public event Action<float> DamageTaken;
        public event Action<float, float> HealthChanged;
        public event Action Died;
    
        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }

        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            CurrentHealth = _maxHealth;
        }
    
        public void RestoreFullHealth()
        {
            if (IsDead)
                return;
            if (CurrentHealth == MaxHealth)
                return;
            var previousHealth = CurrentHealth;
            CurrentHealth = MaxHealth;
            HealthChanged?.Invoke(previousHealth, CurrentHealth);
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0f)
                return;
            if (IsDead)
                return;
        
            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            var actualDamage = previousHealth - CurrentHealth;
        
            HealthChanged?.Invoke(previousHealth, CurrentHealth);
            DamageTaken?.Invoke(actualDamage);
            if (IsDead) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
                return;
            if (IsDead)
                return;
            if (CurrentHealth == MaxHealth)
                return;
        
            var previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        
            HealthChanged?.Invoke(previousHealth, CurrentHealth);
        }
    }
}
