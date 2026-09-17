using System;
using UnityEngine;

namespace Asset.Script.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField]
        private int _maxHealth = 3;

        private int _currentHealth;

        /// <summary>
        /// Current Health, Max Health
        /// </summary>
        public event Action<int, int> HealthChanged;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {

        }

        public void TakeDamage(int damage)
        {
            _currentHealth = Mathf.Clamp(_currentHealth -  damage, 0, _maxHealth);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // 게임오버 추가
        }
    }

}