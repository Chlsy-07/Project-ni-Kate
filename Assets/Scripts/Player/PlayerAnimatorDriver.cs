using UnityEngine;

namespace ProjectNiKate.Player
{
    /// <summary>
    /// Reads state from PlayerController every frame and pushes it into the
    /// Animator. Also handles SpriteRenderer.flipX so a single right-facing 
    /// sprite sheet is all that is needed.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerAnimatorDriver : MonoBehaviour
    {
        private PlayerController _controller;
        private Animator         _animator;
        private SpriteRenderer   _spriteRenderer;

        private static readonly int SpeedHash       = Animator.StringToHash("speed");
        private static readonly int IsGroundedHash  = Animator.StringToHash("isGrounded");
        private static readonly int VelocityYHash   = Animator.StringToHash("velocityY");
        private static readonly int JumpTriggerHash = Animator.StringToHash("jumpTrigger");

        // Debounce: prevents 1-frame Idle flicker when the character stops.
        private float _reportedSpeed;
        private float _lowSpeedTimer;
        private const float LOW_SPEED_TOLERANCE = 0.05f;

        private void Awake()
        {
            _controller     = GetComponent<PlayerController>();
            _animator       = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _controller.OnJumped += HandleJump;
        }

        private void OnDisable()
        {
            _controller.OnJumped -= HandleJump;
        }

        private void Update()
        {
            float actualSpeed = _controller.HorizontalSpeed;

            // Debounce: only report speed=0 after a brief settling window
            // to prevent 1-frame Idle flicker on direction change.
            if (actualSpeed < 0.1f)
            {
                _lowSpeedTimer += Time.deltaTime;
                if (_lowSpeedTimer > LOW_SPEED_TOLERANCE)
                    _reportedSpeed = actualSpeed;
            }
            else
            {
                _lowSpeedTimer = 0f;
                _reportedSpeed = actualSpeed;
            }

            _animator.SetFloat(SpeedHash,      _reportedSpeed);
            _animator.SetBool (IsGroundedHash, _controller.IsGrounded);
            _animator.SetFloat(VelocityYHash,  _controller.VelocityY);

            // Flip the SpriteRenderer — no need for a separate left-facing sheet.
            // flipX false = facing right (default sheet direction)
            // flipX true  = facing left
            _spriteRenderer.flipX = !_controller.IsFacingRight;
        }

        private void HandleJump()
        {
            _animator.SetTrigger(JumpTriggerHash);
        }
    }
}
