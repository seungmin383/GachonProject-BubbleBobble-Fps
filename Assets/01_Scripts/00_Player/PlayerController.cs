using UnityEngine;
using Asset.Script.Manager;

namespace Asset.Script.Player
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerMovement _movement;
        private PlayerLook _look;
        private BubbleAttack _bubbleAttack;

        private PlayerInputReader _inputReader;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();

            _movement = GetComponent<PlayerMovement>();
            _look = GetComponent<PlayerLook>();
            _bubbleAttack = GetComponent<BubbleAttack>();
        }

        private void Update()
        {
            if (GameManager.Instance.IsPaused)
            {
                return; 
            }

            if(_inputReader.PressedJump)
            {
                _movement.Jump();
            }

            if(_inputReader.LeftMouseInput)
            {
                _bubbleAttack.Attack();
            }

            _movement.Movement(_inputReader.CurrentInput);
            _look.Look(_inputReader.CurrentLook);
        }
    }
}