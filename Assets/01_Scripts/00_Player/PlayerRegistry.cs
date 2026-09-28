using System;
using UnityEngine;

namespace Asset.Script.Player
{
    public static class PlayerRegistry
    {
        public static PlayerController CurrentPlayerController { get; private set; }
        public static event Action<PlayerController> PlayerChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            CurrentPlayerController = null;
            PlayerChanged = null;
        }

        public static void Register(PlayerController controller)
        {
            if (controller == null || !controller.isActiveAndEnabled || ReferenceEquals(CurrentPlayerController, controller))
            {
                return;
            }

            CurrentPlayerController = controller;
            PlayerChanged?.Invoke(CurrentPlayerController);
        }

        public static void Unregister(PlayerController player)
        {
            if (ReferenceEquals(player, null) || !ReferenceEquals(CurrentPlayerController, player))
            {
                return;
            }

            CurrentPlayerController = null;
            PlayerChanged?.Invoke(null);
        }
    }
}
