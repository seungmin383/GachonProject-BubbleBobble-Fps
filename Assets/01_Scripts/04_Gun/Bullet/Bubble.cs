using UnityEngine;
using Asset.Script.Monster;
using Unity.VisualScripting;

namespace Asset.Script.Weapon
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _lifeTime = 5.0f;

        private float _elapsedTime;
        private BasicMonster _monster;

        void Update()
        {
            Move();
            UpdateLifeTime();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<BasicMonster>(out BasicMonster basicMonster))
            {
                basicMonster.Capture(gameObject);
                _monster = basicMonster;
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
                if( _monster != null )
                {
                    _monster.Chain();
                }

                Destroy(gameObject);
            }
        }
    }
}