using UnityEngine;

namespace Asset.Script.Manager
{

    public class GameManager : MonoBehaviour
    {
        private bool _isPaused;

        private void Awake()
        {
            CursorManager.Lock();
            InputManager.Initialize();
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
        private void OnDestroy()
        {
            InputManager.Release();
        }

        private void Pause()
        {
            Time.timeScale = 0.0f;
            _isPaused = true;

            CursorManager.Unlock();
        }

        private void Resume()
        {
            Time.timeScale = 1.0f;
            _isPaused = false;

            CursorManager.Lock();
        }
    }
}
