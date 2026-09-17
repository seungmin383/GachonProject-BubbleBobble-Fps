using Asset.Script.Player;
using UnityEngine;

namespace Asset.Script.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] 
        private PlayerHealth _playerHealth;
        [SerializeField] 
        private GameObject[] _hpIconPoints;

        private void OnEnable()
        {
            _playerHealth.HealthChanged += Refresh;
            Refresh(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
        private void OnDisable()
        {
            _playerHealth.HealthChanged -= Refresh;
        }

        private void Refresh(int currentHealth, int maxHealth)
        {
            for (int i = 0; i < _hpIconPoints.Length; i++)
            {
                _hpIconPoints[i].SetActive(i < currentHealth);
            }
        }
    }

}