using System.Collections;
using UnityEngine;

namespace Components.Weapons
{
    [RequireComponent(typeof(Weapon))]
    
    public class WeaponVisualFeedback : MonoBehaviour
    {
        [SerializeField] private GameObject _muzzleFlash;
        [SerializeField] private Sprite _sprite;
        [SerializeField, Min(0f)] private float _flashDuration = 0.05f;
        
        private SpriteRenderer _spriteRenderer;
        private Weapon _weapon;
        private Coroutine _muzzleFlashCoroutine;

        private void Awake()
        {
            _weapon = GetComponent<Weapon>();

            if (_muzzleFlash == null)
                throw new MissingReferenceException($"{name} is missing a muzzle flash.");
            
            _spriteRenderer = _muzzleFlash.GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _sprite;
        }

        private void OnWeaponFired()
        {
            if (_muzzleFlashCoroutine != null)
                StopCoroutine(_muzzleFlashCoroutine);

            _muzzleFlashCoroutine = StartCoroutine(ShowMuzzleFlash());
        }

        private IEnumerator ShowMuzzleFlash()
        {
            _muzzleFlash.SetActive(true);

            yield return new WaitForSeconds(_flashDuration);

            _muzzleFlash.SetActive(false);

            _muzzleFlashCoroutine = null;
        }
        
        private void OnEnable()
        {
            _weapon.WeaponFired += OnWeaponFired;
        }

        private void OnDisable()
        {
            _weapon.WeaponFired -= OnWeaponFired;
        }
    }
}