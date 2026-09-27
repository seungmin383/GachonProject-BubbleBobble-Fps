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
            /* GPT-버블 만료로 탈출하면 몬스터의 분노 행동을 시작한다. */
            _capturable.Escaped += OnEscaped;
        }
        private void OnDisable()
        {
            _capturable.Captured -= OnCaptured;
            _capturable.BubbleBurst -= OnBubbleBurst;
            /* GPT-비활성화 후 탈출 이벤트가 남아 상태를 변경하지 않도록 구독을 해제한다. */
            _capturable.Escaped -= OnEscaped;
        }

        protected override void Idle()
        {

        }
        protected override void Chase() 
        {
            LookAtTarget();
            /* GPT-기본 속도에 분노 배율을 적용해 반복 탈출하더라도 이동속도가 2배를 넘지 않게 한다. */
            float speed = _moveSpeed * (_capturable.IsAngry ? 2.0f : 1.0f);
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        /* GPT-분노 중에도 같은 추적 경로를 사용하며 이동속도만 2배로 적용한다. */
        protected override void Angry() => Chase();

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

        /* GPT-탈출 시 포획 상태를 해제하고 분노 상태로 전환한다. */
        private void OnEscaped() => _currentState = MonsterState.Angry;

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
                /* GPT-공격이나 재포획을 거친 뒤에도 분노한 몬스터는 Angry 추적으로 돌아간다. */
                _currentState = _capturable.IsAngry ? MonsterState.Angry : MonsterState.Chase;
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
