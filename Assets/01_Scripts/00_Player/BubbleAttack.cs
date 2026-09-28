using UnityEngine;

namespace Asset.Script.Player
{
    public class BubbleAttack : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bullet;

        [SerializeField]
        private Transform _firePosition;

        [SerializeField, Min(0.01f)] private float _fireInterval = 0.25f;
        private float _nextFireTime;

        public void Attack()
        {
            if (Time.time < _nextFireTime)
            {
                return;
            }

            _nextFireTime = Time.time + _fireInterval;
            Instantiate(_bullet, _firePosition.position, _firePosition.rotation);
        }
    }
}
