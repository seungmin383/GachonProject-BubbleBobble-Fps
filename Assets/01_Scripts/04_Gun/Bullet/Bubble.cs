using UnityEngine;
using Asset.Script.Interfaces;
using System.Collections;

namespace Asset.Script.Weapon
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField]
        private float _lifeTime = 10.0f;
        [SerializeField]
        private float _angryLifeTime = 5.0f;
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _floatingSpeed = 3.0f;
        [SerializeField]
        private float _timeToFloating = 3.0f;
        [SerializeField]
        private float _floatingTransitionTime = 1.0f;
        [SerializeField]
        private float _chainRadius = 2.0f;
        [SerializeField]
        private float _delayTime = 0.1f;

        private ICapturable _capturable;

        private float _currentLifeTime;
        private float _remainingTime;
        public float RemainingTime => Mathf.Max(0.0f, _remainingTime);
        private State _currentState = State.Flying;
        private Vector3 _moveDirection;
        private float _floatingElapsedTime;

        private bool _isBurst;
        private bool _burstScheduled;

        private enum State
        {
            Flying, Floating, Captured, Popping, Bursting
        }

        private void Awake()
        {
            _moveDirection = transform.forward;
            _remainingTime = _lifeTime;
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
                    _currentState = State.Captured;

                    if(other.TryGetComponent<MonsterBase>(out var monster))
                    {
                        _remainingTime = monster.IsAngry ? _angryLifeTime : _lifeTime;
                    }
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
            _currentLifeTime += Time.deltaTime;

            _remainingTime -= Time.deltaTime;

            /* Fly -> Floating */
            if(_currentLifeTime > _timeToFloating && _currentState == State.Flying)
            {
                _currentState = State.Floating;
            }

            /* Expire */
            if (_remainingTime <= 0.0f)
            {
                Finish(true);
            }
        }

        public void Burst() => Finish(false);
        public void Expire() => Finish(true);

        private void Finish(bool expired)
        {
            if (_isBurst)
                return;

            _isBurst = true;
            _currentState = State.Bursting;

            if (expired)
            {
                _capturable?.Escape();
            }
            else
            {
                _capturable?.OnBubbleBurst();

                BurstNearbyBubbles();
            }

            _capturable = null;

            Destroy(gameObject);
        }

        private void AdaptState()
        {
            switch(_currentState)
            {
                case State.Flying:
                    Move();
                    break;

                case State.Floating:
                    MoveFloating();
                    break;

                case State.Captured:
                    MoveFloating();
                    break;

                case State.Popping:
                    break;

                case State.Bursting:
                    break;
            }
        }

        private void BurstNearbyBubbles()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _chainRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                if(hit.TryGetComponent<Bubble>(out var bubble) && bubble != null)
                {
                    bubble.ScheduleBurst(_delayTime);
                }
            }
        }

        public void ScheduleBurst(float delay)
        {
            if (_isBurst || _burstScheduled)
            {
                return;
            }

            _burstScheduled = true;

            StartCoroutine(BurstDelayed(delay));
        }

        /* GPT-연쇄 폭발 사이에 짧은 간격을 두어 전파 과정을 볼 수 있게 한다. */
        private IEnumerator BurstDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Burst();
        }
    }
}
