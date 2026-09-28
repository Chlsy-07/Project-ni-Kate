using System;
using UnityEngine;

namespace ProjectNiKate.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        // ── Movement ──────────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float _runSpeed     = 6f;
        [SerializeField] private float _acceleration = 60f;
        [SerializeField] private float _deceleration = 80f;

        // ── Jump ──────────────────────────────────────────────────────────────
        [Header("Jump")]
        [SerializeField] private float _jumpForce             = 12f;
        [SerializeField] private float _coyoteTime            = 0.1f;
        [SerializeField] private float _jumpBufferTime        = 0.1f;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;

        // ── Ground Detection ──────────────────────────────────────────────────
        [Header("Grounding")]
        [SerializeField] private float     _groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask _groundLayer;

        // ── Internal References ───────────────────────────────────────────────
        private Rigidbody2D   _rb;
        private BoxCollider2D _col;

        // ── Raw Input State ───────────────────────────────────────────────────
        private float _moveInput;
        private bool  _isHoldingA;
        private bool  _isHoldingD;

        // ── Jump Timers ───────────────────────────────────────────────────────
        private float _coyoteCounter;
        private float _jumpBufferCounter;

        // ── Public State (consumed by PlayerAnimatorDriver) ───────────────────
        public bool  IsGrounded    { get; private set; }
        public bool  IsFacingRight { get; private set; } = true;

        /// <summary>Horizontal speed magnitude — always >= 0.</summary>
        public float HorizontalSpeed => Mathf.Abs(_rb.linearVelocity.x);
        public float VelocityY       => _rb.linearVelocity.y;

        /// <summary>Fires on the exact frame a jump executes.</summary>
        public event Action OnJumped;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            _rb  = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
        }

        // Input polling always in Update — GetKeyDown is silently lost in FixedUpdate.
        private void Update()
        {
            GatherInput();
            UpdateGroundAndTimers();
            HandleJump();
        }

        private void FixedUpdate()
        {
            ApplyHorizontalMovement();
            ApplyFallGravity();
            ClampToBounds();
        }

        private void ClampToBounds()
        {
            if (ProjectNiKate.Gameplay.Level.LevelBoundsManager.Instance != null && ProjectNiKate.Gameplay.Level.LevelBoundsManager.Instance.HasBounds)
            {
                Bounds b = ProjectNiKate.Gameplay.Level.LevelBoundsManager.Instance.CurrentBounds;
                Vector2 pos = _rb.position;

                // Stop the player precisely at the edge so they don't fall off
                if (pos.x < b.min.x)
                {
                    pos.x = b.min.x;
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                }
                else if (pos.x > b.max.x)
                {
                    pos.x = b.max.x;
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                }

                _rb.position = pos;
            }
        }

        // ── Input ─────────────────────────────────────────────────────────────
        private void GatherInput()
        {
            _isHoldingA = Input.GetKey(KeyCode.A);
            _isHoldingD = Input.GetKey(KeyCode.D);

            // ── Direction & facing (free in air per spec) ──
            _moveInput = 0f;

            if (_isHoldingA && !_isHoldingD)
            {
                _moveInput    = -1f;
                IsFacingRight = false;
            }
            else if (_isHoldingD && !_isHoldingA)
            {
                _moveInput    = 1f;
                IsFacingRight = true;
            }

            // ── Jump buffer ──
            if (Input.GetKeyDown(KeyCode.W))
                _jumpBufferCounter = _jumpBufferTime;
        }

        // ── Ground & Timers ───────────────────────────────────────────────────
        private void UpdateGroundAndTimers()
        {
            // Derive check position from the exact bottom of the BoxCollider2D.
            Vector2 checkPos = (Vector2)transform.position
                               + _col.offset
                               + new Vector2(0f, -_col.size.y / 2f);

            // Filter: exclude own collider and triggers.
            Collider2D validHit = null;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, _groundCheckRadius, _groundLayer);
            foreach (Collider2D h in hits)
            {
                if (h.transform.root != transform.root && !h.isTrigger)
                {
                    validHit = h;
                    break;
                }
            }

            // If moving upward faster than 0.1, we cannot be grounded.
            if (_rb.linearVelocity.y > 0.1f)
                IsGrounded = false;
            else
                IsGrounded = validHit != null;

            if (IsGrounded)
                _coyoteCounter = _coyoteTime;
            else
                _coyoteCounter -= Time.deltaTime;

            _jumpBufferCounter -= Time.deltaTime;
        }

        // ── Jump ──────────────────────────────────────────────────────────────
        private void HandleJump()
        {
            if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                _jumpBufferCounter = 0f;
                _coyoteCounter     = 0f;
                IsGrounded         = false; // Prevent Animator from cancelling jump on same frame
                OnJumped?.Invoke();
            }

            // Variable height: releasing W early cuts the arc
            if (Input.GetKeyUp(KeyCode.W) && _rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
        }

        // ── Horizontal Movement ───────────────────────────────────────────────
        private void ApplyHorizontalMovement()
        {
            float target = _moveInput * _runSpeed;
            float rate   = Mathf.Abs(target) > 0.01f ? _acceleration : _deceleration;

            float newVelocityX = Mathf.MoveTowards(_rb.linearVelocity.x, target, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newVelocityX, _rb.linearVelocity.y);
        }

        // ── Fall Gravity ──────────────────────────────────────────────────────
        private void ApplyFallGravity()
        {
            if (_rb.linearVelocity.y < 0f)
            {
                _rb.linearVelocity += Vector2.up
                                      * Physics2D.gravity.y
                                      * (_fallGravityMultiplier - 1f)
                                      * Time.fixedDeltaTime;
            }
        }

        // ── Respawn ───────────────────────────────────────────────────────────
        /// <summary>
        /// Instantly moves the player to <paramref name="position"/> and zeroes all momentum.
        /// Uses _rb.position (physics-layer move) so the engine respects the
        /// teleport on the same frame and doesn't apply residual velocity.
        /// Called by RespawnManager.
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.position       = position;
            _jumpBufferCounter = 0f;
            _coyoteCounter     = 0f;
        }

        // ── Gizmos ────────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            if (_col == null) return;
            Vector2 checkPos = (Vector2)transform.position
                               + _col.offset
                               + new Vector2(0f, -_col.size.y / 2f);
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPos, _groundCheckRadius);
        }
    }
}
