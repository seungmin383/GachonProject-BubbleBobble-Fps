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

        private bool _wasKinematic;

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

        public void Escape()
        {
            if (_bubble == null)
                return;

            _bubble = null;
            if (_rigidBody != null)
                _rigidBody.isKinematic = _wasKinematic;

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
