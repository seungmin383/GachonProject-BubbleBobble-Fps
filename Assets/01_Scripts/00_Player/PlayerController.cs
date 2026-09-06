using UnityEngine;
using Asset.Script.Player;

namespace Asset.Script.Player
{

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement     _movement;
        [SerializeField] private PlayerLock         _lock;


        private void Awake()
        {
            _movement   = GetComponent<PlayerMovement>();
            _lock       = GetComponent<PlayerLock>();
        }

        private void Start()
        {
        
        }


        private void Update()
        {
        
        }
    }
}