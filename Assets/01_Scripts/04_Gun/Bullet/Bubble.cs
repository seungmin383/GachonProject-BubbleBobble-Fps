using UnityEngine;
using Asset.Script.Monster;
using Unity.VisualScripting;
using Asset.Script.Interfaces;

namespace Asset.Script.Weapon
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _lifeTime = 5.0f;

        private float _elapsedTime;
        private bool _isBurst;

        private ICapturable _capturable;

        void Update()
        {
            Move();
            UpdateLifeTime();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ICapturable>(out ICapturable captured))
            {
                if(_capturable != null || _isBurst)
                {
                    return;
                }

                if(captured.TryCapture(this))
                {
                    _capturable = captured;
                }
            }
        }

        private void Move()
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        private void UpdateLifeTime()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > _lifeTime)
            {
                Burst();
            }
        }
        public void Burst()
        {
            if (_isBurst)
                return;

            _isBurst = true;

            _capturable?.OnBubbleBurst();
            _capturable = null;

            Destroy(gameObject);
        }
    }
}