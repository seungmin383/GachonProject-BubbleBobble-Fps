using UnityEngine;
using Asset.Script.Weapon;

namespace Asset.Script.Interfaces
{
    public interface ICapturable
    {
        bool TryCapture(Bubble bubble);
        void OnBubbleBurst();
    }
}