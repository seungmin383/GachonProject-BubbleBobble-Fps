using UnityEngine;

public abstract class MonsterBase : MonoBehaviour
{
    protected enum MonsterState
    {
        Idle, Chase, Attack, Captured, Dead
    }

    protected MonsterState _currentState = MonsterState.Idle;

    public virtual void Update()
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
    protected virtual void Attack() { }
    protected virtual void Captured() { }

    protected virtual void Die()
    {
        _currentState = MonsterState.Dead;

        // 추후 pool 반환
        Destroy(gameObject);
    }
}
