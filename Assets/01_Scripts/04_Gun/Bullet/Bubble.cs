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
                if(_capturable != null)
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

            if(_elapsedTime > _lifeTime )
            {
                _elapsedTime = 0;

                // 몬스터가 먼저 파괴됐지만 인터페이스 참조가 남은 경우는 확인 불가
                _capturable?.OnBubbleBurst();
                _capturable = null;

                // 추후 Pool 에 넣는 식으로 변경 예정
                Destroy(gameObject);
            }
        }
    }
}