using Asset.Script.Interfaces;
using Asset.Script.Weapon;
using UnityEngine;

namespace Asset.Script.Monster
{

    public class BasicMonster : MonoBehaviour, ICapturable
    {
        private Bubble _bubble;
        private Rigidbody _rigidBody;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            FollowBubble();
        }

        public bool TryCapture(Bubble bubble)
        {
            if( _bubble == null && bubble != null)
            {
                _bubble = bubble;
                _rigidBody.isKinematic = true;

                return true;
            }
            else
            {
                return false;
            }
        }

        void ICapturable.OnBubbleBurst()
        {
            _bubble = null;
            Die();
        }

        private void FollowBubble()
        {
            if (_bubble == null)
            {
                return;
            }

            transform.position = _bubble.transform.position;
        }

        private void Die()
        {
            // 추후 Pool 에 넣는 식으로 변경 예정
            Destroy(gameObject);
        }
    }
}