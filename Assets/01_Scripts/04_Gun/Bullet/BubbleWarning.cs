using UnityEngine;

namespace Asset.Script.Weapon
{
    [RequireComponent(typeof(Bubble))]
    public class BubbleWarning : MonoBehaviour
    {
        [SerializeField] private string _colorProperty = "_BaseColor";
        [SerializeField, Min(0.01f)] private float _slowInterval = 0.5f;
        [SerializeField, Min(0.01f)] private float _fastInterval = 0.1f;

        private Bubble _bubble;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        private Color _originalColor;

        private int _colorId;
        private float _elapsed;
        private float _previousTime = float.PositiveInfinity;
        private bool _isRed;

        private void Awake()
        {
            _bubble = GetComponent<Bubble>();
            _renderer = GetComponent<Renderer>();
            _colorId = Shader.PropertyToID(_colorProperty);

            if (_renderer == null || _renderer.sharedMaterial == null || !_renderer.sharedMaterial.HasProperty(_colorId))
            {
                enabled = false;
                return;
            }

            _originalColor = _renderer.sharedMaterial.GetColor(_colorId);
            _properties = new MaterialPropertyBlock();
        }

        private void Update()
        {
            float remaining = _bubble.RemainingTime;

            if (remaining > 5.0f)
            {
                _elapsed = 0.0f;
                _isRed = false;
            }
            else if (_previousTime > 5.0f || remaining > _previousTime)
            {
                _elapsed = 0.0f;
                _isRed = true;
            }
            else
            {
                float interval = Mathf.Lerp(_slowInterval, _fastInterval, Mathf.InverseLerp(5.0f, 1.0f, remaining));
                _elapsed += Time.deltaTime;
                if (_elapsed >= interval)
                {
                    _elapsed = 0.0f;
                    _isRed = !_isRed;
                }
            }

            _previousTime = remaining;
            SetColor(_isRed ? new Color(1.0f, 0.0f, 0.0f, _originalColor.a) : _originalColor);
        }

        /* 공유 머티리얼을 수정하지 않고 이 버블의 색상만 변경하며 다른 프로퍼티는 보존한다. */
        private void SetColor(Color color)
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetColor(_colorId, color);
            _renderer.SetPropertyBlock(_properties);
        }

        private void OnDisable()
        {
            _previousTime = float.PositiveInfinity;
            _elapsed = 0.0f;
            _isRed = false;
            if (_properties != null && _renderer != null)
                SetColor(_originalColor);
        }
    }
}
