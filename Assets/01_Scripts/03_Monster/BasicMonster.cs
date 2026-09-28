using Asset.Script.Component;
using Asset.Script.Manager;
using Asset.Script.Player;
using UnityEngine;

namespace Asset.Script.Monster
{
    [RequireComponent(typeof(Capturable))]
    public class BasicMonster : MonsterBase
    {
        [SerializeField]
        private Capturable _capturable;

        private Transform _target;
        private PlayerHealth _targetHealth;
        [SerializeField]
        private float _moveSpeed = 3.0f;
        [SerializeField]
        private float _attackRange = 1.5f;
        [SerializeField]
        private int _attackDamage = 1;
        [SerializeField]
        private float _attackInterval = 5.0f;
        [SerializeField]
        private float _angrySpeedMagnification = 2.0f;

        private float _attackElapsedTime;

        private void Awake()
        {
            if (_capturable == null)
            {
                _capturable = GetComponent<Capturable>();
            }
        }

        protected override void Update()
        {
            if (PauseService.IsPaused)
            {
                return;
            }

            _attackElapsedTime += Time.deltaTime;
            UpdateState();

            base.Update();
        }

        private void OnEnable()
        {
            _capturable.Captured += OnCaptured;
            _capturable.BubbleBurst += OnBubbleBurst;

            _capturable.Escaped += OnEscaped;

            PlayerRegistry.PlayerChanged += BindPlayer;
            BindPlayer(PlayerRegistry.CurrentPlayerController);
        }
        private void OnDisable()
        {
            if (_capturable != null)
            {
                _capturable.Captured -= OnCaptured;
                _capturable.BubbleBurst -= OnBubbleBurst;
                _capturable.Escaped -= OnEscaped;
            }

            PlayerRegistry.PlayerChanged -= BindPlayer;
            BindPlayer(null);
        }

        private void BindPlayer(PlayerController controller)
        {
            _target         = controller != null ? controller.transform : null;
            _targetHealth   = controller != null ? controller.Health : null;
            UpdateState();
        }

        protected override void Idle()
        {


        }
        protected override void Chase() 
        {
            MoveToTarget(_moveSpeed);
        }

        protected override void Angry()
        {
            MoveToTarget(_moveSpeed * _angrySpeedMagnification);
        }

        protected override void Attack() 
        {
            if (_target == null || _targetHealth == null)
            {
                return;
            }

            LookAtTarget();

            if( _attackElapsedTime < _attackInterval )
            {
                return;
            }

            _attackElapsedTime = 0.0f;

            _targetHealth.TakeDamage(_attackDamage);
        }
        protected override void Captured() 
        {

        }

        private void OnCaptured()
        {
            _currentState = MonsterState.Captured;
        }
        private void OnBubbleBurst()
        {
            Die();
        }

        private void OnEscaped()
        {
            IsAngry = true;
            _currentState = MonsterState.Angry;
        }

        private void UpdateState()
        {
            if(_currentState == MonsterState.Captured || _currentState == MonsterState.Dead)
            {
                return;
            }

            if (_target == null || _targetHealth == null)
            {
                _currentState = MonsterState.Idle;
                return;
            }

            float distance = Vector3.Distance(transform.position, _target.position);

            if (distance < _attackRange)
            {
                _currentState = MonsterState.Attack;
            }
            else
            {
                _currentState = IsAngry ? MonsterState.Angry : MonsterState.Chase;
            }
        }

        private void LookAtTarget()
        {
            Vector3 direction = _target.position - transform.position;
            direction.y = 0.0f;
            if (direction.sqrMagnitude <= 0.001f)
                return;
            direction.Normalize();
            transform.forward = direction;
        }

        private void MoveToTarget(float speed)
        {
            LookAtTarget();
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
