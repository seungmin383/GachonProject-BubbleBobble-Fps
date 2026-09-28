using UnityEngine;
using Asset.Script.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Asset.Script.Weapon
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bubble : MonoBehaviour
    {
        [SerializeField]
        private float _lifeTime = 10.0f;
        [SerializeField]
        private float _angryLifeTime = 5.0f;
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _floatingSpeed = 3.0f;
        [SerializeField]
        private float _timeToFloating = 3.0f;
        [SerializeField]
        private float _floatingTransitionTime = 1.0f;
        [SerializeField]
        private float _chainRadius = 2.0f;
        [SerializeField]
        private float _delayTime = 0.1f;

        [SerializeField, Min(0.0f)] 
        private float _flightPushImpulse = 1.0f;
        [SerializeField, Min(0.0f)] 
        private float _ceilingPushImpulse = 1.5f;
        [SerializeField, Min(0.01f)]
        private float _floatingSteering = 3.0f;

        [SerializeField] 
        private SphereCollider _bodyCollider;
        /* GPT-비행 경로에서는 벽, 바닥, 천장만 검사하고 버블과 포획 대상은 기존 충돌 규칙으로 처리한다. */
        [SerializeField]
        private LayerMask _environmentLayers;

        private ICapturable _capturable;
        private Rigidbody _rigidBody;
        private static bool _collisionLayersConfigured;
        private readonly HashSet<Collider> _ceilingContacts = new HashSet<Collider>();

        private float _currentLifeTime;
        private float _remainingTime;
        public float RemainingTime => Mathf.Max(0.0f, _remainingTime);
        private MotionState _motionState = MotionState.Flying;
        private Vector3 _moveDirection;
        private float _floatingElapsedTime;

        private bool _isBurst;
        private bool _burstScheduled;

        private enum MotionState
        {
            Flying, Floating, CeilingBlocked
        }

        /* 런타임 로드가 꺼져있을때 static 값 초기화해주는 메서드 */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCollisionLayerConfiguration()
        {
            _collisionLayersConfigured = false;
        }

        #region Unity Function
        private void Awake()
        {
            _moveDirection = transform.forward;
            _remainingTime = _lifeTime;
            _rigidBody = GetComponent<Rigidbody>();

            if (_bodyCollider == null || !ConfigureCollisionLayers())
            {
#if UNITY_EDITOR
                Debug.LogError("Bubble body collider or physics layers are missing.", this);
#endif
                enabled = false;
                return;
            }

            /* GPT-기존 버블 인스턴스에도 현재 맵의 Default와 Wall 레이어를 환경 검사 대상으로 적용한다. */
            if (_environmentLayers.value == 0)
            {
                _environmentLayers = LayerMask.GetMask("Default", "Wall");
            }

            _rigidBody.useGravity = false;
            _rigidBody.isKinematic = true;
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void Update()
        {
            UpdateState();
        }

        private void FixedUpdate()
        {
            if (_isBurst)
                return;

            if (_motionState == MotionState.Flying)
            {
                /* GPT-비행 이동 전에 환경 접촉을 검사해 Kinematic 버블이 벽과 천장을 통과하지 않게 한다. */
                MoveFlying();
            }
            else if (_motionState == MotionState.Floating)
            {
                MoveFloating();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ICapturable>(out ICapturable captured))
            {
                if(_capturable != null || _isBurst)
                {
                    return;
                }

                if(captured.TryCapture(this))
                {
                    _capturable = captured;
                    BeginFloating();

                    if(other.TryGetComponent<MonsterBase>(out var monster))
                    {
                        _remainingTime = monster.IsAngry ? _angryLifeTime : _lifeTime;
                    }
                }
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (_isBurst)
            {
                return;
            }

            Bubble otherBubble = collision.collider.GetComponentInParent<Bubble>();
            if (otherBubble != null && otherBubble != this)
            {
                if (_motionState == MotionState.Flying && otherBubble._motionState != MotionState.Flying)
                {
                    otherBubble.ReceivePush(_moveDirection, _flightPushImpulse);
                }
                else if (_motionState == MotionState.Floating && otherBubble._motionState == MotionState.CeilingBlocked)
                {
                    Vector3 sideways = otherBubble.transform.position - transform.position;
                    sideways.y = 0.0f;

                    if (sideways.sqrMagnitude < 0.001f)
                    {
                        sideways = Vector3.ProjectOnPlane(_moveDirection, Vector3.up);
                    }

                    otherBubble.ReceivePush(sideways, _ceilingPushImpulse);
                }
                return;
            }
            TrackCeilingContact(collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            if (_isBurst || collision.collider.GetComponentInParent<Bubble>() != null)
            {
                return;
            }

            TrackCeilingContact(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_ceilingContacts.Remove(collision.collider) && _ceilingContacts.Count == 0)
            {
                ResumeFloating();
            }
        }
        #endregion

        #region Personal Function

        /* 상태 갱신해주는 함수 */
        private void UpdateState()
        {
            _currentLifeTime += Time.deltaTime;

            _remainingTime -= Time.deltaTime;

            if (_currentLifeTime > _timeToFloating && _motionState == MotionState.Flying)
            {
                BeginFloating();
            }

            /* Expire */
            if (_remainingTime <= 0.0f)
            {
                Finish(true);
            }
        }

        public void Burst() => Finish(false);
        public void Expire() => Finish(true);
        /* 터뜨리는 함수 */
        private void Finish(bool expired)
        {
            if (_isBurst)
                return;

            _isBurst = true;

            if (expired)
            {
                _capturable?.Escape();
            }
            else
            {
                _capturable?.OnBubbleBurst();

                BurstNearbyBubbles();
            }

            _capturable = null;

            Destroy(gameObject);
        }

        /* GPT-버블의 실제 크기로 이번 물리 단계의 이동 경로를 검사해 빠른 비행도 얇은 환경 면을 통과하지 않게 한다. */
        private void MoveFlying()
        {
            Vector3 scale = _bodyCollider.transform.lossyScale;
            float radius = _bodyCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Vector3 centerOffset = _bodyCollider.transform.TransformPoint(_bodyCollider.center) - transform.position;
            Vector3 center = _rigidBody.position + centerOffset;
            if (TryHandleEnvironmentOverlap(center, radius))
            {
                return;
            }

            Vector3 movement = transform.forward * _speed * Time.fixedDeltaTime;
            float distance = movement.magnitude;
            if (distance > 0.0f && Physics.SphereCast(center, radius, movement / distance,
                out RaycastHit hit, distance, _environmentLayers, QueryTriggerInteraction.Ignore))
            {
                HandleEnvironmentContact(hit.collider, hit.normal, _rigidBody.position + movement.normalized * hit.distance);
                return;
            }

            _rigidBody.MovePosition(_rigidBody.position + movement);
        }

        /* GPT-SphereCast가 감지하지 못하는 시작 지점의 겹침도 확인해 벽 안에서 발사되면 터지고 천장 안에서는 아래로 보정한다. */
        private bool TryHandleEnvironmentOverlap(Vector3 center, float radius)
        {
            if (!Physics.CheckSphere(center, radius, _environmentLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            Collider ceiling = null;
            Vector3 ceilingNormal = Vector3.zero;
            Vector3 ceilingCorrection = Vector3.zero;
            float deepestOverlap = 0.0f;
            Vector3 bodyPosition = _rigidBody.position + _bodyCollider.transform.position - transform.position;
            Collider[] overlaps = Physics.OverlapSphere(center, radius, _environmentLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider environment in overlaps)
            {
                if (!Physics.ComputePenetration(_bodyCollider, bodyPosition, _bodyCollider.transform.rotation,
                    environment, environment.transform.position, environment.transform.rotation,
                    out Vector3 normal, out float depth))
                {
                    continue;
                }

                /* GPT-천장과 벽에 동시에 겹쳤다면 벽이나 바닥의 즉시 폭발 규칙을 우선한다. */
                if (normal.y >= -0.6f)
                {
                    Burst();
                    return true;
                }

                if (depth > deepestOverlap)
                {
                    ceiling = environment;
                    ceilingNormal = normal;
                    ceilingCorrection = normal * depth;
                    deepestOverlap = depth;
                }
            }

            if (ceiling == null)
            {
                return false;
            }

            HandleEnvironmentContact(ceiling, ceilingNormal, _rigidBody.position + ceilingCorrection);
            return true;
        }

        /* GPT-비행 중 벽과 바닥에 닿으면 폭발시키고, 천장 아래쪽 면에 닿으면 기존 천장 정지 상태로 연결한다. */
        private void HandleEnvironmentContact(Collider environment, Vector3 normal, Vector3 contactPosition)
        {
            if (normal.y >= -0.6f)
            {
                Burst();
                return;
            }

            /* GPT-상승 상태로 바꾼 뒤 Transform과 물리 위치를 함께 맞춰 높이 고정 시 이전 비행 위치로 되돌아가지 않게 한다. */
            BeginFloating();
            transform.position = contactPosition;
            _rigidBody.position = contactPosition;
            /* GPT-천장 도착 시 비행 방향의 속도를 없애고 천장 접촉을 등록해 이후 옆으로 밀려 벗어나면 다시 상승하게 한다. */
            _rigidBody.linearVelocity = Vector3.zero;
            _ceilingContacts.Add(environment);
            BlockAtCeiling();
        }

        /* 떠오르는 움직임 함수 */
        private void MoveFloating()
        {
            _floatingElapsedTime += Time.fixedDeltaTime;

            float t = _floatingElapsedTime / _floatingTransitionTime;
            t = Mathf.Clamp01(t);

            Vector3 direction = Vector3.Slerp(_moveDirection, Vector3.up, t);
            Vector3 desiredVelocity = direction * _floatingSpeed;
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            float steering = 1.0f - Mathf.Exp(-_floatingSteering * Time.fixedDeltaTime);

            _rigidBody.linearVelocity = new Vector3(
                Mathf.Lerp(currentVelocity.x, desiredVelocity.x, steering),
                desiredVelocity.y,
                Mathf.Lerp(currentVelocity.z, desiredVelocity.z, steering));
        }

        /* 떠오르기 시작할 때 값 세팅해주는 함수 */
        private void BeginFloating()
        {
            if (_motionState != MotionState.Flying)
                return;

            _motionState = MotionState.Floating;
            _floatingElapsedTime = 0.0f;
            _bodyCollider.gameObject.layer = LayerMask.NameToLayer("FloatingBubbleBody");
            _rigidBody.isKinematic = false;
            _rigidBody.linearVelocity = _moveDirection * _floatingSpeed;
        }

        /* 최초의 레이어 세팅 함수 */
        private bool ConfigureCollisionLayers()
        {
            int flyingLayer     = LayerMask.NameToLayer("FlyingBubbleBody");
            int floatingLayer   = LayerMask.NameToLayer("FloatingBubbleBody");
            int playerLayer     = LayerMask.NameToLayer("Player");
            int monsterLayer    = LayerMask.NameToLayer("Monster");
            int wallLayer       = LayerMask.NameToLayer("Wall");

            if (flyingLayer < 0 || floatingLayer < 0 || playerLayer < 0 || monsterLayer < 0 || wallLayer < 0)
            {
                return false;
            }

            if (!_collisionLayersConfigured)
            {
                Physics.IgnoreLayerCollision(flyingLayer, flyingLayer, true);
                Physics.IgnoreLayerCollision(flyingLayer, floatingLayer, false);
                Physics.IgnoreLayerCollision(flyingLayer, LayerMask.NameToLayer("Default"), true);
                Physics.IgnoreLayerCollision(flyingLayer, wallLayer, true);
                Physics.IgnoreLayerCollision(flyingLayer, playerLayer, true);
                Physics.IgnoreLayerCollision(flyingLayer, monsterLayer, true);

                Physics.IgnoreLayerCollision(floatingLayer, floatingLayer, false);
                Physics.IgnoreLayerCollision(floatingLayer, LayerMask.NameToLayer("Default"), false);
                Physics.IgnoreLayerCollision(floatingLayer, wallLayer, false);
                Physics.IgnoreLayerCollision(floatingLayer, playerLayer, true);
                Physics.IgnoreLayerCollision(floatingLayer, monsterLayer, true);

                _collisionLayersConfigured = true;
            }

            _bodyCollider.gameObject.layer = flyingLayer;
            return true;
        }
            
        /* 천장 바닥이랑 닿았는지 확인하는 함수 */
        private void TrackCeilingContact(Collision collision)
        {
            if (_motionState == MotionState.Flying)
            {
                return;
            }

            bool touchesCeiling = false;
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y < -0.6f)
                {
                    touchesCeiling = true;
                    break;
                }
            }

            if (touchesCeiling)
            {
                _ceilingContacts.Add(collision.collider);
                if (_motionState == MotionState.Floating)
                    BlockAtCeiling();
            }
            else if (_ceilingContacts.Remove(collision.collider) && _ceilingContacts.Count == 0)
            {
                ResumeFloating();
            }
        }
        /* 천장에 닿으면 멈추는 함수 */
        private void BlockAtCeiling()
        {
            _motionState = MotionState.CeilingBlocked;
            Vector3 velocity = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(velocity.x, 0.0f, velocity.z);
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        /* 다시 떠오르게 처리하는 함수 */
        private void ResumeFloating()
        {
            if (_motionState != MotionState.CeilingBlocked)
                return;

            _motionState            = MotionState.Floating;
            _floatingElapsedTime    = _floatingTransitionTime;
            _rigidBody.constraints  = RigidbodyConstraints.FreezeRotation;
            Vector3 velocity        = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(velocity.x, _floatingSpeed, velocity.z);
        }

        /* direction 방향으로 impulse 만큼 미는 함수 */
        private void ReceivePush(Vector3 direction, float impulse)
        {
            if (_isBurst || _motionState == MotionState.Flying || impulse <= 0.0f)
            {
                return;
            }

            if (_motionState == MotionState.CeilingBlocked)
            {
                direction.y = 0.0f;
            }
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.ProjectOnPlane(_moveDirection, Vector3.up);
            }
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.right;
            }

            _rigidBody.AddForce(direction.normalized * impulse, ForceMode.Impulse);
        }

        /* 주변 버블 터뜨리는 Trigger */
        private void BurstNearbyBubbles()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _chainRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                Bubble bubble = hit.GetComponentInParent<Bubble>();
                if (bubble != null && bubble != this)
                {
                    bubble.ScheduleBurst(_delayTime);
                }
            }
        }

        /* delay 터지게 설정 */
        public void ScheduleBurst(float delay)
        {
            if (_isBurst || _burstScheduled)
            {
                return;
            }

            _burstScheduled = true;
            StartCoroutine(BurstDelayed(delay));
        }

        /* 시간 지나면 터지는 코드 */
        private IEnumerator BurstDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Burst();
        }
        #endregion

#if UNITY_EDITOR
        /* Editor에서 터지는 범위 확인 */
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _chainRadius);
        }
#endif
    }
}
