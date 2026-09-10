using Asset.Script.Manager;
using UnityEngine;

namespace Asset.Script.UI
{
    public class Setting : MonoBehaviour
    {
        [SerializeField]
        private GameObject _panel;

        private void Start()
        {
            GameManager.Instance.PauseEvent += OnPauseChanged;
        }

        private void OnDisable() 
        {
            GameManager.Instance.PauseEvent -= OnPauseChanged;
        }

        private void OnPauseChanged(bool isPaused)
        {
            _panel.SetActive(isPaused);
        }
    }
}