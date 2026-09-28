using UnityEngine;
using Asset.Script.Player;

namespace Asset.Script.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        private PlayerInputAction _inputAction;

        private bool _gameplayInputEnabled = true;
        public bool GameplayInputEnabled => isActiveAndEnabled && _gameplayInputEnabled && _inputAction != null && _inputAction.Player.enabled;

        /* 입력 없음 → ( 0, 0)
            W        → ( 0, 1)
            S        → ( 0,-1)
            A        → (-1, 0)
            D        → ( 1, 0) */
        public Vector2 CurrentInput => GameplayInputEnabled ? _inputAction.Player.Move.ReadValue<Vector2>() : Vector2.zero;
        public bool PressedJump => GameplayInputEnabled && _inputAction.Player.Jump.WasPressedThisFrame();

        /* 마우스 오른쪽 이동 → (양수, 0)
           마우스 왼쪽 이동   → (음수, 0)
           마우스 위 이동     → (0, 양수)
           마우스 아래 이동   → (0, 음수)*/
        public Vector2 CurrentLook => GameplayInputEnabled ? _inputAction.Player.Look.ReadValue<Vector2>() : Vector2.zero;

        public bool LeftMouseInput => GameplayInputEnabled && _inputAction.Player.LeftMouseInput.IsPressed();
        public bool RightMouseInput => GameplayInputEnabled && _inputAction.Player.RightMouseInput.WasPressedThisFrame();

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            ApplyInputState();
        }

        private void OnDisable()
        {
            _inputAction?.Player.Disable();
        }

        public void SetGameplayInputEnabled(bool isEnabled)
        {
            _gameplayInputEnabled = isEnabled;
            EnsureInitialized();
            ApplyInputState();
        }

        private void EnsureInitialized()
        {
            if (_inputAction == null)
            {
                _inputAction = new PlayerInputAction();
            }
        }

        private void ApplyInputState()
        {
            if (isActiveAndEnabled && _gameplayInputEnabled)
            {
                _inputAction.Player.Enable();
            }
            else
            {
                _inputAction.Player.Disable();
            }
        }

        private void OnDestroy()
        {
            _inputAction?.Dispose();
            _inputAction = null;
        }
    }

}
