/* GPT-헤드리스 검증에서 URP의 색상 프로퍼티와 동일한 이름으로 개별 렌더러 색상 변경을 확인한다. */
Shader "Validation/Color"
{
    Properties { _BaseColor ("Color", Color) = (0.3, 1, 0.9, 0.2) }
    SubShader { Pass { } }
}
