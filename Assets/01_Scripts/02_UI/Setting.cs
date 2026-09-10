using Asset.Script.Manager;
using UnityEngine;

namespace Asset.Script.UI
{
    public class Setting : MonoBehaviour
    {
        [SerializeField]
        private GameObject _panel;

        private GameManager _gameManager;

        private void Start()
        {
            _gameManager = GameManager.Instance;
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable() 
        {
            if(null == _gameManager)
            {
                return;
            }
            _gameManager.PauseEvent -= OnPauseChanged;
        }

        private void Subscribe()
        {
            if (_gameManager == null)
                return;

            _gameManager.PauseEvent += OnPauseChanged;
            OnPauseChanged(_gameManager.IsPaused);
        }

        private void OnPauseChanged(bool isPaused)
        {
            _panel.SetActive(isPaused);
        }
    }
}