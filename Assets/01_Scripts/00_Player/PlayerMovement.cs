using UnityEngine;
using Asset.Script.Player;
using Asset.Script.ClientDefine;
using Unity.VisualScripting.Dependencies.Sqlite;

namespace Asset.Script.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header(" 플레이어 스탯 ")]

        [SerializeField]
        private float _speed;


        [Header(" 외부 변수 ")]

        [SerializeField]
        [Range(0.0f, 100.0f)]
        private float _gravityForce = 9.8f;
        [SerializeField]
        [Range(-10.0f, 0.0f)]
        private float _downForce = -2.0f; 
        [SerializeField]
        [Range(0.0f, 100.0f)]
        private float _jumpForce = 5.0f;


        private CharacterController _characterController;

        private float _verticalVelocity;


        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// 외부 입력 방향에 따라 이동하기 위한 코드입니다.
        /// </summary>
        /// <param name="direction"> 비정규화 값을 넣어주세요 </param>
        public void Movement(Vector2 direction)
        {
            /* 수평 이동 */
            Vector3 horizontalMovement 
                = transform.forward * direction.y + transform.right * direction.x;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1.0f) * _speed;

            /* 수직 이동 */
            _verticalVelocity -= _gravityForce * Time.deltaTime; 
            Vector3 verticalMovement = Vector3.up * _verticalVelocity;
            
            /* 땅에 있는가 확인 */
            if (_characterController.isGrounded && _verticalVelocity < 0.0f)
            {
                _verticalVelocity = _downForce;
            }

            /* 최종 이동 */
            Vector3 finalMovement = horizontalMovement + verticalMovement;
            _characterController.Move(finalMovement * Time.deltaTime);
        }

        public void Jump()
        {
            if (_characterController.isGrounded)
            {
                /* 중력 적용 */
                _verticalVelocity = _jumpForce;
            }
        }
    }
}