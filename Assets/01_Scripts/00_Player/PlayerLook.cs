using UnityEngine;
using Asset.Script.Player;

namespace Asset.Script.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("컴포넌트")]
        [SerializeField]
        private Transform _cameraTransform;

        [Header("내부 변수")]
        [SerializeField]
        private float _sensitivity = 0.1f;

        private float _pitch;

        public void Look(Vector2 look)
        {            
            float yaw = look.x * _sensitivity;
            transform.Rotate(Vector3.up, yaw);

            _pitch -= look.y * _sensitivity;
            _pitch = Mathf.Clamp(_pitch, -90.0f, 90.0f);

            _cameraTransform.localRotation = Quaternion.Euler(_pitch, 0.0f, 0.0f);
        }
    }
}