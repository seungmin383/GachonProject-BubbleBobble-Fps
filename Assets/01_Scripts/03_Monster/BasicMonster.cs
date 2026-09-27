using Asset.Script.Component;
using Asset.Script.Interfaces;
using Asset.Script.Player;
using Asset.Script.Weapon;
using Unity.VisualScripting;
using UnityEngine;

namespace Asset.Script.Monster
{

    public class BasicMonster : MonsterBase
    {
        [SerializeField]
        private Capturable _capturable;

        [SerializeField]
        private Transform _target;
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


        protected override void Update()
        {
            _attackElapsedTime += Time.deltaTime;
            UpdateState();

            base.Update();
        }

        private void OnEnable()
        {
            _capturable.Captured += OnCaptured;
            _capturable.BubbleBurst += OnBubbleBurst;

            _capturable.Escaped += OnEscaped;
        }
        private void OnDisable()
        {
            _capturable.Captured -= OnCaptured;
            _capturable.BubbleBurst -= OnBubbleBurst;

            _capturable.Escaped -= OnEscaped;
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
            LookAtTarget();

            if( _attackElapsedTime < _attackInterval )
            {
                return;
            }

            _attackElapsedTime = 0.0f;

            if(_target.TryGetComponent<PlayerHealth>(out PlayerHealth health))
            {
                health.TakeDamage(_attackDamage);
            }
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
