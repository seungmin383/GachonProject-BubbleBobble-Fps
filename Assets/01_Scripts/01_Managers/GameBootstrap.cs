using UnityEngine;

namespace Asset.Script.Manager
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            _instance = null;
            InputManager.Release();

            PauseService.Reset();
        }

        /* 첫 씬의 Awake보다 먼저 지속되는 시스템 객체를 만들어 시작 씬에 따른 초기화 차이를 없앤다. */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject root = new GameObject(nameof(GameBootstrap));
            root.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InputManager.Initialize();

            if (GameManager.Instance == null)
            {
                gameObject.AddComponent<GameManager>();
            }
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            InputManager.Release();
            PauseService.Reset();
            CursorManager.Unlock();
            _instance = null;
        }
    }
}
