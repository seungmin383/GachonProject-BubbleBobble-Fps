using Asset.Script.Component;
using Asset.Script.Interfaces;
using Asset.Script.Weapon;
using Unity.VisualScripting;
using UnityEngine;

namespace Asset.Script.Monster
{

    public class BasicMonster : MonsterBase
    {
        [SerializeField]
        private Capturable _capturable;

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

        }
        protected override void Attack() 
        {

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
    }
}