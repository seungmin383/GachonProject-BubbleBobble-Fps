using Asset.Script.Weapon;
using UnityEngine;

namespace Asset.Script.Player
{
    public class GunAttack : MonoBehaviour
    {
        [SerializeField]
        private GameObject _aimCamera;
        [SerializeField]
        private float _range = 100.0f;

        public void Attack()
        {
            Ray ray = new Ray(_aimCamera.transform.position, _aimCamera.transform.forward);

#if UNITY_EDITOR
            Debug.DrawRay(_aimCamera.transform.position, _aimCamera.transform.forward * _range, Color.red, 2f);
#endif
            if (Physics.Raycast(ray, out RaycastHit hit, _range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent<Bubble>(out var bubble))
                {
                    bubble.Burst();
                }
            }
        }
    }
}
