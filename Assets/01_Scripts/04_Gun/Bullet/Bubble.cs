using UnityEngine;
using Asset.Script.Interfaces;

namespace Asset.Script.Weapon
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _floatingSpeed = 3.0f;
        [SerializeField]
        private float _lifeTime = 20.0f;

        [SerializeField]
        private float _timeToFloating = 3.0f;

        private float _elapsedTime;
        private bool _isBurst;
        private BubbleState _currentState = BubbleState.Flying;
        
        [SerializeField]
        private float _floatingTransitionTime = 1.0f;

        private Vector3 _moveDirection;
        private float _floatingElapsedTime;

        private ICapturable _capturable;

        private enum BubbleState
        {
            Flying, Floating, Captured, Popping, Bursting
        }

        private void Start()
        {
            _moveDirection = transform.forward;
        }

        void Update()
        {
            UpdateState();
            AdaptState();
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
                    _currentState = BubbleState.Captured;
                }
            }
        }

        private void Move()
        {
            transform.position += transform.forward * _speed * Time.deltaTime;
        }
        private void MoveFloating()
        {
            _floatingElapsedTime += Time.deltaTime;

            float t = _floatingElapsedTime / _floatingTransitionTime;
            t = Mathf.Clamp01(t);

            Vector3 direction = Vector3.Slerp(_moveDirection, Vector3.up, t);
            transform.position += direction * _floatingSpeed * Time.deltaTime;
        }

        private void UpdateState()
        {
            _elapsedTime += Time.deltaTime;

            if(_elapsedTime > _timeToFloating && _currentState == BubbleState.Flying)
            {
                _currentState = BubbleState.Floating;
            }

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
            _currentState = BubbleState.Bursting;

            // 만약 버블이 무언가를 잡고있다면
            _capturable?.OnBubbleBurst();
            _capturable = null;

            Destroy(gameObject);
        }

        private void AdaptState()
        {
            switch(_currentState)
            {
                case BubbleState.Flying:
                    Move();
                    break;

                case BubbleState.Floating:
                    MoveFloating();
                    break;

                case BubbleState.Captured:
                    MoveFloating();
                    break;

                case BubbleState.Popping:
                    break;

                case BubbleState.Bursting:
                    break;

            }
        }
    }
}