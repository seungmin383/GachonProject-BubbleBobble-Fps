using UnityEngine;

namespace Asset.Script.Manager
{
    [DisallowMultipleComponent]
    public sealed class BattleSceneContext : MonoBehaviour
    {
        public static BattleSceneContext Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState() => Current = null;

        private void OnEnable()
        {
            if (Current != null && Current != this)
            {
                enabled = false;
                return;
            }

            Current = this;
            PauseService.Reset();
            PauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(PauseService.IsPaused);
            GameManager.Instance?.BeginBattle();
        }

        private void Update()
        {
            if (InputManager.PressedPause)
            {
                PauseService.Toggle();
            }
        }

        private void OnDisable()
        {
            if (Current != this)
            {
                return;
            }

            PauseService.PauseChanged -= OnPauseChanged;
            Current = null;
            PauseService.Reset();
            CursorManager.Unlock();
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (isPaused)
            {
                CursorManager.Unlock();
            }
            else
            {
                CursorManager.Lock();
            }
        }
    }
}
