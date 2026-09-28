using System;
using UnityEngine;

namespace Asset.Script.Manager
{
    public static class PauseService
    {
        public static bool IsPaused { get; private set; }
        public static event Action<bool> PauseChanged;
        private static float _resumeTimeScale = 1.0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            PauseChanged = null;
            Reset();
        }

        public static void Pause() => SetPaused(true);
        public static void Resume() => SetPaused(false);
        public static void Toggle() => SetPaused(!IsPaused);

        public static void Reset()
        {
            bool wasPaused = IsPaused;
            IsPaused = false;
            _resumeTimeScale = 1.0f;
            Time.timeScale = 1.0f;

            if (wasPaused)
            {
                PauseChanged?.Invoke(false);
            }
        }

        private static void SetPaused(bool isPaused)
        {
            if (IsPaused == isPaused)
            {
                return;
            }

            if (isPaused)
            {
                _resumeTimeScale = Time.timeScale;
                Time.timeScale = 0.0f;
            }
            else
            {
                Time.timeScale = _resumeTimeScale;
            }

            IsPaused = isPaused;
            PauseChanged?.Invoke(isPaused);
        }
    }
}
