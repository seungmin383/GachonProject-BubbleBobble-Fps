using UnityEngine;
using Asset.Script.Interfaces;

namespace Asset.Script.Weapon
{
    public class Bubble : MonoBehaviour
    {
        /* GPT-일반 버블은 10초, 분노한 적을 포획한 버블은 5초 뒤 만료되도록 시간을 설정한다. */
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

        private ICapturable _capturable;

        private float _currentLifeTime;
        /* GPT-이동 경과 시간과 만료 시간을 분리하고 점멸 컴포넌트에 남은 시간을 제공한다. */
        private float _remainingTime;
        public float RemainingTime => Mathf.Max(0.0f, _remainingTime);
        private State _currentState = State.Flying;
        private Vector3 _moveDirection;
        private float _floatingElapsedTime;

        private bool _isBurst;

        private enum State
        {
            Flying, Floating, Captured, Popping, Bursting
        }

        /* GPT-생성 직후 수명을 초기화해 포획이나 점멸이 시작되기 전에 타이머를 준비한다. */
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
                    /* GPT-비행한 시간과 무관하게 포획 순간부터 일반 적은 10초, 분노한 적은 5초를 보장한다. */
                    _remainingTime = captured.IsAngry ? _angryLifeTime : _lifeTime;
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
            /* GPT-일시정지 중에는 줄어들지 않는 별도 타이머로 버블의 만료를 판정한다. */
            _remainingTime -= Time.deltaTime;

            if(_currentLifeTime > _timeToFloating && _currentState == State.Flying)
            {
                _currentState = State.Floating;
            }

            /* GPT-시간 만료는 적을 처치하는 Pop 대신 탈출 경로로 종료한다. */
            if (_remainingTime <= 0.0f)
            {
                Finish(true);
            }
        }

        /* GPT-총기 Pop은 기존 호출 이름을 유지하면서 적 처치 경로로 종료한다. */
        public void Burst() => Finish(false);

        /* GPT-만료와 Pop의 중복 처리를 막고 원인에 따라 탈출 또는 처치한 뒤 버블을 제거한다. */
        private void Finish(bool expired)
        {
            if (_isBurst)
                return;

            _isBurst = true;
            _currentState = State.Bursting;

            if (expired)
                _capturable?.Escape();
            else
                _capturable?.OnBubbleBurst();
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
    }
}
