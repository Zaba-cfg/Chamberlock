using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.Game
{
    public class PauseInput : MonoBehaviour
    {
        private const string PauseActionPath = "Player/Pause";

        public static PauseInput Instance { get; private set; }

        public event Action PausePressed;

        private PlayerInputActions _actions;
        private InputAction _pauseAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one PauseInput is allowed.");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _actions = new PlayerInputActions();

            // Resolved by name so this script compiles before the action is added to the asset,
            // and fails fast with a clear message if the action was never created.
            _pauseAction = _actions.asset.FindAction(PauseActionPath, throwIfNotFound: false);

            if (_pauseAction == null)
                throw new MissingReferenceException(
                    $"{name}: no action '{PauseActionPath}'. Add a 'Pause' action to the 'Player' map in PlayerInputActions.");
        }

        private void OnEnable()
        {
            if (_pauseAction == null)
                return;

            _pauseAction.performed += OnPausePerformed;
            _pauseAction.Enable();
        }

        private void OnDisable()
        {
            if (_pauseAction == null)
                return;

            _pauseAction.performed -= OnPausePerformed;
            _pauseAction.Disable();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            _actions?.Dispose();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            PausePressed?.Invoke();
        }
    }
}
