using Asset.Script.Manager;
using UnityEngine;

namespace Asset.Script.UI
{
    public class Setting : MonoBehaviour
    {
        [SerializeField]
        private GameObject _panel;

        private void OnEnable()
        {
            PauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(PauseService.IsPaused);
        }

        private void OnDisable() 
        {
            PauseService.PauseChanged -= OnPauseChanged;
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (_panel != null)
            {
                _panel.SetActive(isPaused);
            }
        }
    }
}
