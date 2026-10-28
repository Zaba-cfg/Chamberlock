using Systems.Audio;
using UnityEngine;

namespace Components.Base.HealthRelated
{
    [RequireComponent(typeof(Health))]
    
    public class HealthAudioFeedback : MonoBehaviour
    {
        [SerializeField] private AudioClip _damageClip;
        [SerializeField] private AudioClip _deathClip;
        
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            
            if (_damageClip == null)
                throw new MissingReferenceException($"{name} is missing damage clip.");

            if (_deathClip == null)
                throw new MissingReferenceException($"{name} is missing death clip.");
        }

        private void OnDamageTaken(float damage)
        {
            AudioManager.Instance.PlaySfx(_damageClip);
        }

        private void OnDied()
        {
            AudioManager.Instance.PlaySfx(_deathClip);
        }
        
        private void OnEnable()
        {
            _health.DamageTaken += OnDamageTaken;
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.DamageTaken -= OnDamageTaken;
            _health.Died -= OnDied;
        }
    }
}