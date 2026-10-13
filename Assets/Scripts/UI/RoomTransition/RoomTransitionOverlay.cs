using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.RoomTransition
{
    [RequireComponent(typeof(CanvasGroup))]

    public class RoomTransitionOverlay : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.15f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_image == null)
                throw new MissingReferenceException($"{name} is missing its Image.");
        }

        private void Start()
        {
            StartCoroutine(FadeIn());
        }

        public IEnumerator FadeOut()
        {
            yield return FadeTo(1f);
        }

        public IEnumerator FadeIn()
        {
            yield return FadeTo(0f);
        }

        private IEnumerator FadeTo(float target)
        {
            _canvasGroup.blocksRaycasts = target > 0f;

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, target, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = target;
        }
    }
}