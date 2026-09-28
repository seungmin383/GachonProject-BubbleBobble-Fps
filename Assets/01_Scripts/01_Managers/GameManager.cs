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
        
        private GameState _currentState;
        public GameState CurrentState => _currentState;

        private int _currentCoin;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance()
        {
            Instance = null;
        }

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

        public void BeginBattle() => ChangeState(GameState.Playing);

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                return;
            }

            if (BattleSceneContext.Current != null && BattleSceneContext.Current.gameObject.scene == scene)
            {
                BeginBattle();
            }
            else
            {
                ChangeState(scene.name == SceneNames.Loading ? GameState.Loading : GameState.WaitingForStart);
            }
        }

        private void OnDestroy()
        {
            if(this != Instance)
            {
                return;
            }

            Instance = null;
        }
    }
}
