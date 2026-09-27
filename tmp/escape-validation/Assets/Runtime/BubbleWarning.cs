/* GPT-버블의 남은 시간에 따른 색상 경고를 게임 규칙과 분리해 렌더러에만 적용한다. */
using UnityEngine;

namespace Asset.Script.Weapon
{
    [RequireComponent(typeof(Bubble))]
    public class BubbleWarning : MonoBehaviour
    {
        /* GPT-셰이더 색상 프로퍼티와 점멸 간격을 Inspector에서 조정할 수 있게 한다. */
        [SerializeField] private string _colorProperty = "_BaseColor";
        [SerializeField, Min(0.01f)] private float _slowInterval = 0.5f;
        [SerializeField, Min(0.01f)] private float _fastInterval = 0.1f;

        /* GPT-원래 색과 점멸 진행을 버블별로 보관해 공유 머티리얼이나 다른 버블에 영향을 주지 않는다. */
        private Bubble _bubble;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        private Color _originalColor;
        private int _colorId;
        private float _elapsed;
        private float _previousTime = float.PositiveInfinity;
        private bool _isRed;

        /* GPT-현재 머티리얼의 색상과 투명도를 저장하고 해당 셰이더가 색상 변경을 지원할 때만 점멸한다. */
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

        /* GPT-남은 5초부터 빨간색 경고를 시작하고 1초까지 가속하며 포획으로 시간이 늘어나면 점멸을 초기화한다. */
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

        /* GPT-공유 머티리얼을 수정하지 않고 이 버블의 색상만 변경하며 다른 프로퍼티는 보존한다. */
        private void SetColor(Color color)
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetColor(_colorId, color);
            _renderer.SetPropertyBlock(_properties);
        }

        /* GPT-경고 컴포넌트를 끄거나 다시 켤 때 빨간색과 이전 점멸 진행이 남지 않도록 복구한다. */
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
