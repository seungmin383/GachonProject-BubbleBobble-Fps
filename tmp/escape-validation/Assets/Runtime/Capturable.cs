using Asset.Script.Interfaces;
using Asset.Script.Weapon;
using System;
using UnityEngine;

namespace Asset.Script.Component
{

    public class Capturable : MonoBehaviour, ICapturable
    {
        private Bubble _bubble;
        private Rigidbody _rigidBody;

        /* GPT-포획 전 물리 설정을 복구하고 여러 번 탈출해도 분노 여부를 하나의 값으로 유지한다. */
        private bool _wasKinematic;
        public bool IsAngry { get; private set; }
        public event Action Escaped;

        public event Action Captured;
        public event Action BubbleBurst;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            FollowBubble();
        }


        public void OnBubbleBurst()
        {
            _bubble = null;

            BubbleBurst?.Invoke();
        }

        /* GPT-만료 시 따라가기를 해제하고 물리 설정을 복구한 뒤 분노 상태로 탈출했음을 알린다. */
        public void Escape()
        {
            if (_bubble == null)
                return;

            _bubble = null;
            if (_rigidBody != null)
                _rigidBody.isKinematic = _wasKinematic;

            IsAngry = true;
            Escaped?.Invoke();
        }

        public bool TryCapture(Bubble bubble)
        {
            if (bubble == null || _bubble != null)
            {
                return false;
            }

            _bubble = bubble;

            if (_rigidBody != null)
            {
                /* GPT-원래부터 Kinematic인 대상도 탈출 후 원래 물리 설정을 유지하도록 현재 값을 저장한다. */
                _wasKinematic = _rigidBody.isKinematic;
                _rigidBody.isKinematic = true;
            }

            Captured?.Invoke();
            
            return true;
        }

        private void FollowBubble()
        {
            if (_bubble == null)
            {
                return;
            }

            transform.position = _bubble.transform.position;
        }
    }

}
