using System;
using UnityEngine;

namespace Asset.Script.Manager
{
    public class GameManager : MonoBehaviour
    {
        private bool _isPaused;
        public bool IsPaused => _isPaused;

        public static GameManager Instance { get; private set; }

        public event Action<bool> PauseEvent;

        private void Awake()
        {
            if (null != Instance && this != Instance)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InputManager.Initialize();

            // 나중에 로비 생기면 전투 진입시 Lock으로 변경
            CursorManager.Lock();
        }

        private void Update()
        {
            if(InputManager.PressedPause)
            {
                if(_isPaused )
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        private void Pause()
        {
            Time.timeScale = 0.0f;
            _isPaused = true;

            CursorManager.Unlock();

            PauseEvent?.Invoke(true);
        }

        private void Resume()
        {
            Time.timeScale = 1.0f;
            _isPaused = false;

            CursorManager.Lock();

            PauseEvent?.Invoke(false);
        }

        private void OnDestroy()
        {
            if(this != Instance)
            {
                return;
            }

            InputManager.Release();
            Instance = null;
        }
    }
}
