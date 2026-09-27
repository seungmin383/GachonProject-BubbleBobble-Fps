using UnityEngine;

public abstract class MonsterBase : MonoBehaviour
{
    protected enum MonsterState
    {
        /* GPT-탈출한 몬스터의 빠른 추적 행동을 분노 상태로 구분한다. */
        Idle, Chase, Attack, Captured, Dead, Angry
    }

    protected MonsterState _currentState = MonsterState.Idle;

    protected virtual void Update()
    {
        AdaptState();
    }

    protected void AdaptState()
    {
        switch (_currentState)
        {
            case MonsterState.Idle:
                Idle();
                break;

            case MonsterState.Chase:
                Chase();
                break;

            /* GPT-분노 상태의 행동을 자식 몬스터가 구현한 Angry 메서드로 전달한다. */
            case MonsterState.Angry:
                Angry();
                break;

            case MonsterState.Attack:
                Attack();
                break;

            case MonsterState.Captured:
                Captured();
                break;

            case MonsterState.Dead:
                break;
        }
    }

    protected virtual void Idle() { }
    protected virtual void Chase() { }
    /* GPT-몬스터별 분노 행동을 정의할 수 있도록 상태 처리 지점을 제공한다. */
    protected virtual void Angry() { }
    protected virtual void Attack() { }
    protected virtual void Captured() { }

    protected virtual void Die()
    {
        _currentState = MonsterState.Dead;

        // 추후 pool 반환
        Destroy(gameObject);
    }
}
