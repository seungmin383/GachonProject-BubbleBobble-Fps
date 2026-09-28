using Asset.Script.Player;
using UnityEngine;

namespace Asset.Script.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        private PlayerHealth _playerHealth;
        [SerializeField] 
        private GameObject[] _hpIconPoints;

        private void OnEnable()
        {
            PlayerRegistry.PlayerChanged += BindPlayer;
            BindPlayer(PlayerRegistry.CurrentPlayerController);
        }
        private void OnDisable()
        {
            PlayerRegistry.PlayerChanged -= BindPlayer;
            BindPlayer(null);
        }

        private void BindPlayer(PlayerController player)
        {
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged -= Refresh;
            }

            _playerHealth = player != null ? player.Health : null;
            if (_playerHealth != null)
            {
                _playerHealth.HealthChanged += Refresh;
                Refresh(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
            else
            {
                Refresh(0, 0);
            }
        }

        private void Refresh(int currentHealth, int maxHealth)
        {
            if (_hpIconPoints == null)
            {
                return;
            }

            for (int i = 0; i < _hpIconPoints.Length; i++)
            {
                if (_hpIconPoints[i] != null)
                {
                    _hpIconPoints[i].SetActive(i < currentHealth);
                }
            }
        }
    }

}
