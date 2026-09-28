using UnityEngine;
using ProjectNiKate.Gameplay.Level;

namespace ProjectNiKate.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);

        [Header("Vertical Bounds")]
        [SerializeField] private bool _useStaticBounds = true;
        [SerializeField] private Vector2 _minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(100f, 20f);
        
        [Header("Horizontal Dynamic Bounds")]
        [SerializeField] private bool _useDynamicBounds = true;

        private Vector3 _velocity = Vector3.zero;
        private Camera _cam;

        private void Awake() => _cam = GetComponent<Camera>();

        public void SetTarget(Transform target) => _target = target;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + (Vector3)_offset;
            targetPos.z = transform.position.z;

            // 1. Clamp Horizontal (X) using Dynamic Level Bounds
            if (_useDynamicBounds && LevelBoundsManager.Instance != null && LevelBoundsManager.Instance.HasBounds)
            {
                Bounds b = LevelBoundsManager.Instance.CurrentBounds;
                if (b.size.sqrMagnitude > 0)
                {
                    float halfWidth = _cam.orthographicSize * _cam.aspect;
                    float boundsMinX = b.min.x + halfWidth;
                    float boundsMaxX = b.max.x - halfWidth;

                    if (boundsMinX > boundsMaxX)
                        targetPos.x = b.center.x;  // Level is narrower than viewport
                    else
                        targetPos.x = Mathf.Clamp(targetPos.x, boundsMinX, boundsMaxX);
                }
            }

            // 2. Clamp Vertical (Y) using Static Bounds (keeps player from seeing too high/low)
            if (_useStaticBounds)
            {
                targetPos.y = Mathf.Clamp(targetPos.y, _minBounds.y, _maxBounds.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
        }
    }
}
