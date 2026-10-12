using System.Collections;
using UnityEngine;

namespace Components.Base.HealthRelated
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Health))]
    
    public class HealthVisualFeedback : MonoBehaviour
    {
        [SerializeField] private Color _damageColor = Color.red;
        [SerializeField, Min(0f)] private float _flashDuration = 0.05f;
        
        private SpriteRenderer _spriteRenderer;
        private Health _health;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _health = GetComponent<Health>();
            _originalColor = _spriteRenderer.color;
        }
        
        private void OnDamageTaken(float _)
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(FlashDamage());
        }
        
        private IEnumerator FlashDamage()
        {
            _spriteRenderer.color = _damageColor;

            yield return new WaitForSeconds(_flashDuration);

            _spriteRenderer.color = _originalColor;

            _flashCoroutine = null;
        }

        private void OnEnable()
        {
            _health.DamageTaken += OnDamageTaken;
        }

        private void OnDisable()
        {
            _health.DamageTaken -= OnDamageTaken;
        }
    }
}