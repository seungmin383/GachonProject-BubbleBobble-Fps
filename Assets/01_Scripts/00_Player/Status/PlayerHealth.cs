using UnityEngine;

namespace Asset.Script.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField]
        private int _maxHealth = 3;

        private int _currentHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }
        private void Update()
        {
            Debug.Log($"Ã¼·Â {_currentHealth}");
        }
        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {

        }
    }

}