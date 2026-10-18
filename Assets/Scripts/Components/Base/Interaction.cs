using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Components.Base
{
    [RequireComponent(typeof(CircleCollider2D))]

    public class Interaction : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private AudioClip _interactClip;
    
        private readonly List<IInteractable> _interactables = new();
    
        private CircleCollider2D _circleCollider;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _circleCollider = GetComponent<CircleCollider2D>();
            _circleCollider.isTrigger = true;
            _circleCollider.radius = _interactionRadius;
        }

        public void Interact()
        {
            if (_interactables.Count <= 0) 
                return;
        
            IInteractable closest = null;
            float closestDistance = float.MaxValue;
        
            foreach (IInteractable interactable in _interactables)
            {
                Component component = interactable as Component;
                float distance = (component.transform.position - transform.position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
            closest?.Interact(gameObject);
            
            if (_interactClip != null)
                _audioSource.PlayOneShot(_interactClip);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IInteractable interactable)
                && !_interactables.Contains(interactable))
            {
                _interactables.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IInteractable interactable)
                && _interactables.Contains(interactable))
            {
                _interactables.Remove(interactable);
            }
        }
    }
}
