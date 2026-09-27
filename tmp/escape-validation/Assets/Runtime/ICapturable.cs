using UnityEngine;
using Asset.Script.Weapon;

namespace Asset.Script.Interfaces
{
    public interface ICapturable
    {
        /* GPT-버블이 몬스터 구현에 의존하지 않고 재포획 시간과 만료 시 탈출을 처리하게 한다. */
        bool IsAngry { get; }
        void Escape();
        bool TryCapture(Bubble bubble);
        void OnBubbleBurst();
    }
}
