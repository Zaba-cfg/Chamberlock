using Components.Base.HealthRelated;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class PlayerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Slider _slider;

        private void Awake()
        {
            if (_health == null)
                throw new MissingReferenceException($"{name}: no Health assigned.");

            if (_slider == null)
                throw new MissingReferenceException($"{name}: no Slider assigned.");
        }

        private void OnEnable()
        {
            _health.HealthChanged += HandleHealthChanged;
        }

        private void Start()
        {
            Refresh(_health.CurrentHealth, _health.MaxHealth);
        }

        private void OnDisable()
        {
            _health.HealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float previousHealth, float currentHealth)
        {
            Refresh(currentHealth, _health.MaxHealth);
        }

        private void Refresh(float currentHealth, float maxHealth)
        {
            _slider.minValue = 0f;
            _slider.maxValue = maxHealth;
            _slider.value = currentHealth;
        }
    }
}
