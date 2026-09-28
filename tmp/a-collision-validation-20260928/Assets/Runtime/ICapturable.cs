using UnityEngine;
using Asset.Script.Weapon;

namespace Asset.Script.Interfaces
{
    public interface ICapturable
    {
        void Escape();
        bool TryCapture(Bubble bubble);
        void OnBubbleBurst();
    }
}
