using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Asset.Script.ClientDefine;

namespace Asset.Script.Manager
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            WaitingForStart, Loading, Playing, WaitingForContinue, GameOver
        }

        public static GameManager Instance { get; private set; }

        public event Action<bool> PauseEvent;

        private bool _isPaused;
        public bool IsPaused => _isPaused;

        private GameState _currentState;
        public GameState CurrentState => _currentState;

        private int _currentCoin;

        private void Awake()
        {
            if (null != Instance && this != Instance)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _currentState = GameState.WaitingForStart;
            DontDestroyOnLoad(gameObject);

            InputManager.Initialize();
        }

        private void Update()
        {
            if(InputManager.PressedInsertCoin)
            {
                _currentCoin++;
            }

            if(InputManager.PressedGameStart)
            {
                if(_currentCoin > 0 && _currentState == GameState.WaitingForStart)
                {
                    ChangeState(GameState.Loading);
                    SceneManager.LoadScene(SceneNames.Loading);
                }
            }   
            
            if(_currentState == GameState.Playing)
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
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void ChangeState(GameState nextState)
        {
            if (_currentState == nextState)
            {
                return;
            }

            _currentState = nextState;
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == SceneNames.Battle)
            {
                ChangeState(GameState.Playing);
                CursorManager.Lock();
            }
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
