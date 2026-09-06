using UnityEngine;
using Asset.Script.Player;

namespace Asset.Script.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed;

        private Vector3 _direction;

        private void Awake()
        {
            _direction = transform.forward;
        }

        void Start()
        {
        
        }

        void Update()
        {

        }
    }
}