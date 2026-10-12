using System;
using UnityEngine;

namespace Systems.Game
{
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        public float ElapsedTime { get; private set; }
        public int Kills { get; private set; }
        public int RoomsCleared { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action RunStarted;
        public event Action<RunStats> RunEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one RunManager is allowed.");

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!IsRunning)
                return;
            
            ElapsedTime += Time.deltaTime;
        }

        public void StartRun()
        {
            ElapsedTime = 0f;
            Kills = 0;
            RoomsCleared = 0;
            IsRunning = true;

            RunStarted?.Invoke();
        }

        public void EndRun()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            RunEnded?.Invoke(new RunStats(ElapsedTime, Kills, RoomsCleared));
        }

        public void AddKill()
        {
            if (!IsRunning)
                return;

            Kills++;
        }

        public void AddRoomCleared()
        {
            if (!IsRunning)
                return;

            RoomsCleared++;
        }
    }
}
