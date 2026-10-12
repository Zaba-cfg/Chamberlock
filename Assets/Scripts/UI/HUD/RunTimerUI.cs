using Systems.Game;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    public class RunTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;

        private void Awake()
        {
            if (_timerText == null)
                throw new MissingReferenceException($"{name}: no TMP text assigned.");
        }

        private void Update()
        {
            var runManager = RunManager.Instance;
            if (runManager == null)
                return;

            _timerText.text = FormatTime(runManager.ElapsedTime);
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
