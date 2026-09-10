using UnityEngine;

namespace Asset.Script.Player
{
    public class BubbleAttack : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bullet;

        [SerializeField]
        private Transform _firePosition;

        public void Attack()
        {
            Instantiate(_bullet, _firePosition.position, _firePosition.rotation);
        }
    }
}
