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
        private float _attackInterval = 1.0f;

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
        }
        private void OnDisable()
        {
            _capturable.Captured -= OnCaptured;
            _capturable.BubbleBurst -= OnBubbleBurst;
        }

        protected override void Idle()
        {

        }
        protected override void Chase() 
        {
            LookAtTarget();
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;
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
                _currentState = MonsterState.Chase;
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
    }
}