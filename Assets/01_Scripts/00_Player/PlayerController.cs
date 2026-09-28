using UnityEngine;
using Asset.Script.Manager;

namespace Asset.Script.Player
{
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMovement), typeof(PlayerLook))]
    [RequireComponent(typeof(BubbleAttack), typeof(GunAttack), typeof(PlayerHealth))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerMovement _movement;
        private PlayerLook _look;
        private BubbleAttack _bubbleAttack;
        private GunAttack _gunAttack;

        private PlayerInputReader _inputReader;

        private bool _canControl = true;
        private bool _isPaused;
        public PlayerHealth Health { get; private set; }

        private void Awake()
        {
            _inputReader    = GetComponent<PlayerInputReader>();
            _movement       = GetComponent<PlayerMovement>();
            _look           = GetComponent<PlayerLook>();
            _bubbleAttack   = GetComponent<BubbleAttack>();
            _gunAttack      = GetComponent<GunAttack>();
            Health          = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            PauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(PauseService.IsPaused);
            PlayerRegistry.Register(this);
        }

        private void OnDisable()
        {
            PlayerRegistry.Unregister(this);
            PauseService.PauseChanged -= OnPauseChanged;

            if (_inputReader != null)
            {
                _inputReader.SetGameplayInputEnabled(false);
            }
        }

        public void SetControlEnabled(bool canControl)
        {
            _canControl = canControl;
            RefreshInputState();
        }

        private void OnPauseChanged(bool isPaused)
        {
            _isPaused = isPaused;
            RefreshInputState();
        }

        private void RefreshInputState()
        {
            if (_inputReader == null)
            {
                return;
            }

            _inputReader.SetGameplayInputEnabled(isActiveAndEnabled && _canControl && !_isPaused);
        }

        private void Update()
        {
            if (!_canControl || _isPaused)
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

            if (_inputReader.RightMouseInput)
            {
                _gunAttack.Attack();
            }

            _movement.Movement(_inputReader.CurrentInput);
            _look.Look(_inputReader.CurrentLook);
        }
    }
}
