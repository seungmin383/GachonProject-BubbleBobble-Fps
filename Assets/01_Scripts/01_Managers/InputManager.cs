using UnityEngine;

namespace Asset.Script.Manager
{
    public static class InputManager
    {
        private static GlobalInputAction _globalInputAction;

        public static bool PressedPause => _globalInputAction.UI.Pause.WasPressedThisFrame();
        public static bool PressedInsertCoin => _globalInputAction.UI.InsertCoin.WasPressedThisFrame();
        public static bool PressedGameStart => _globalInputAction.UI.GameStart.WasPressedThisFrame();
        public static void Initialize()
        {
            _globalInputAction = new GlobalInputAction();
            _globalInputAction.Enable();
        }

        public static void Release()
        {
            _globalInputAction.Disable();
            _globalInputAction.Dispose();
            _globalInputAction = null;    
        }
    }
}