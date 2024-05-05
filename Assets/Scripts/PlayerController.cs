using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Mob/Theo here, The base of this script was build on the TaroDev controller script, be sure to thank him and include in the credits when the game launches https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;
        public bool _playerControl;
        private bool _playerIsDefeated;

        private void Awake()
        {
          _playerIsDefeated = false;
          _playerControl = true;
          _rb = GetComponent<Rigidbody2D>();
          _col = GetComponent<CapsuleCollider2D>();

          _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
          _gravityScale = _stats.FallAcceleration;

          dashState = DashState.Ready;

          GameManager.PlayerDefeatedEvent += PlayerDefeated;
        }

        private void OnDestroy()
        {
          GameManager.PlayerDefeatedEvent -= PlayerDefeated;
        }

        private void Update()
        {
          if (_playerIsDefeated)
            return;

          _time += Time.deltaTime;
          GatherInput();
        }

        private void FixedUpdate()
        {
          if (_playerIsDefeated)
            return;

          CheckCollisions();

          if (_playerControl)
          {
            HandleGravity();
            HandleCrouch();
            HandleJump();

            if (!_crouching)
            {
              HandleDirection();
              WallSlide();
              WallJump();
            }
          }
          HandleDash();

          ApplyMovement();
        }

        private void PlayerDefeated()
        {
          _playerIsDefeated = true;
          _rb.velocity = Vector2.zero;
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.J),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.J),
                DashPressed = Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.K),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (_frameInput.DashPressed)
            {
              _timeDashWasPressed = _time;
              _dashPressed = true;
            }
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;
        private bool _isWalled;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, LayerMask.GetMask("Ground"));
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, LayerMask.GetMask("Ground"));

            _isWalled = CheckIfItsWalled();

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            if (groundHit) _groundHitAfterDash = true;

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        public bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime && !_crouching;
        private void HandleJump()
        {
          _endedJumpEarly = ShouldEndJumpEarly();

          if (!_jumpToConsume && !HasBufferedJump)
            return;

          if (_grounded || CanUseCoyote) ExecuteJump();

          if (_wallSliding || wallJumpBufferTimer > 0f)
            return;

          _jumpToConsume = false;
        }

        private bool ShouldEndJumpEarly()
        {
          return !_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0
              && !_wallJumping && !_wallSliding && wallJumpBufferTimer < 0f;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Crouch

        public bool _crouching = false;
        private void HandleCrouch()
        {
          if (!_grounded || _frameVelocity.y >= 0)
          {
            _crouching = false;
            return;
          }

          _crouching = _frameInput.Move.y < 0;

          if(_crouching)
          {
            _frameVelocity.x = 0;
          }
        }

        #endregion
        #region Horizontal

        private void HandleDirection()
        {
          if (_frameInput.Move.x == 0)
          {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;

            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
          }
          else
          {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
          }
        }

        #endregion

        #region Gravity

        private float _gravityScale;

        private void HandleGravity()
        {
          if (_grounded && _frameVelocity.y <= 0f) // means it's grounded
          {
            _frameVelocity.y = _stats.GroundingForce;
            return;
          }

          var fallSpeed = _gravityScale;
          if (_endedJumpEarly || _frameVelocity.y < 0)
            fallSpeed *= _stats.JumpEndEarlyGravityModifier;

            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, fallSpeed * Time.fixedDeltaTime);
        }

        #endregion

        #region Dash

        public DashState dashState;
        private bool _groundHitAfterDash = true;
        private float _lastDashTime = 0;
        public bool _dashReset = false;
        private bool _dashPressed = false;
        private float _timeDashWasPressed;

        private void HandleDash()
        {
          if (_wallSliding || _wallJumping)
          {
            _dashPressed = false;
            return;
          }

          switch(dashState)
          {
            case DashState.Ready:
              float dashPressedTime = _timeDashWasPressed + _stats.DashBuffer;
              // if (dashPressedTime < _time)
              // {
              //   _dashPressed = false;
              //   _timeDashWasPressed = 0f;
              // }

              if (_dashPressed)
              {
                _dashPressed = false;
                _playerControl = false;
                _dashReset = false;
                _groundHitAfterDash = false;
                _lastDashTime = Time.time;
                Time.timeScale = 1 - _stats.DashSlowOnTime;
                StartCoroutine(DashDelayedExecution());
              }
            break;

            case DashState.Dashing:
              bool dashIsOver = _lastDashTime + _stats.DashDuration <= Time.time;
              _dashPressed = false;
              if (dashIsOver || _wallSliding)
              {
                dashState = DashState.Cooldown;
                _playerControl = true;
                StartCoroutine(DelayToReturnGravity());
              }
            break;

            case DashState.Cooldown:
              _dashPressed = false;
             if (_dashReset || (_groundHitAfterDash && _lastDashTime + _stats.DashCooldown <= Time.time))
              dashState = DashState.Ready;
            break;
          }
        }

        IEnumerator DelayToReturnGravity()
        {
          yield return new WaitForSeconds(_stats.DashGravityReturnDelay * Time.timeScale);
          _gravityScale = _stats.FallAcceleration;
        }
        IEnumerator DashDelayedExecution()
        {
          yield return new WaitForSeconds(_stats.DashInputDelay * Time.timeScale);

          CinemachineShake.Instance.ShakeCamera(_stats.DashShakeIntensity, _stats.DashShakeDuration);
          _gravityScale = 0;
          Time.timeScale = 1f;
          dashState = DashState.Dashing;

          switch(_frameInput.Move)
          {
            case var _ when _frameInput.Move == Vector2.zero: // horizontal dashes without pressing any directions
              _frameVelocity.x = Math.Sign(transform.localScale.x) * _stats.DashSpeed;
              _frameVelocity.y = 0;
            break;

            case var _ when _frameInput.Move.x != 0 && _frameInput.Move.y == 0: // horizontal dashes pressing left or right
              _frameVelocity.x = _frameInput.Move.x * _stats.DashSpeed;
              _frameVelocity.y = 0;
            break;

            case var _ when _frameInput.Move.x == 0 && _frameInput.Move.y != 0: // vertical dashes
              _frameVelocity.x = 0;
              _frameVelocity.y = _frameInput.Move.y * _stats.DashVerticalSpeed;
            break;

            case var _ when _frameInput.Move.x != 0 && _frameInput.Move.y != 0: // diagonal dashes
              _frameVelocity.x = _frameInput.Move.x * _stats.DashSpeed;
              _frameVelocity.y = _frameInput.Move.y * _stats.DashVerticalSpeed * 0.5f;
            break;
          }
        }
        #endregion

        #region wallJump

        public bool _wallSliding;
        public bool _wallJumping;
        private float wallJumpDirection;
        private float wallJumpBufferTimer;

        private bool CheckIfItsWalled()
        {
          Vector2 lookingDirection = Vector2.right * Math.Sign(transform.localScale.x) * (_col.size.x / 1.9f);
          Vector2 middlePos = _col.bounds.center;
          Vector2 topPos = new Vector2(_col.bounds.center.x, _col.bounds.center.y + transform.localScale.y / 1.5f);
          Vector2 bottomPos = new Vector2(_col.bounds.center.x, _col.bounds.center.y - transform.localScale.y / 1.5f);

          bool middleCheck = Physics2D.Linecast(middlePos, middlePos + lookingDirection, LayerMask.GetMask("Ground"));
          bool topCheck = Physics2D.Linecast(topPos, topPos + lookingDirection, LayerMask.GetMask("Ground"));
          bool bottomCheck = Physics2D.Linecast(bottomPos, bottomPos + lookingDirection, LayerMask.GetMask("Ground"));

          #if UNITY_EDITOR // this is just so the lines can be visualized as Gizmos while playing
          Color drawColor = middleCheck ? Color.green : Color.red;
          Debug.DrawLine(middlePos, middlePos + lookingDirection, drawColor);

          drawColor = topCheck ? Color.green : Color.red;
          Debug.DrawLine(topPos, topPos + lookingDirection, drawColor);

          drawColor = bottomCheck ? Color.green : Color.red;
          Debug.DrawLine(bottomPos, bottomPos + lookingDirection, drawColor);
          #endif

          return middleCheck && topCheck && bottomCheck;
        }

        private void WallSlide()
        {
          if (_isWalled && !_grounded && _frameInput.Move.x != 0)
          {
            _wallSliding = true;
            _frameVelocity.y = Mathf.Clamp(_frameVelocity.y, -_stats.WallSlidingSpeed, float.MaxValue);
          }
          else
          {
            _wallSliding = false;
          }
        }

        private void WallJump()
        {
          if (_grounded)
          {
            wallJumpBufferTimer = 0f;
            return;
          }

          if (_wallSliding)
          {
            _wallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpBufferTimer = _stats.WallJumpBuffer;

            CancelInvoke(nameof(StopWallJumping));
          }
          else
          {
            wallJumpBufferTimer -= Time.deltaTime;
          }

          if (_jumpToConsume && wallJumpBufferTimer > 0f)
          {
            _wallJumping = true;
            _frameVelocity = new Vector2(wallJumpDirection * _stats.WallJumpPower.x, _stats.WallJumpPower.y);
            wallJumpBufferTimer = 0f;

            if (transform.localScale.x != wallJumpDirection)
            {
              Vector3 localScale = transform.localScale;
              localScale.x *= -1f;
              transform.localScale = localScale;
            }
          }

          Invoke(nameof(StopWallJumping), _stats.WallJumpDuration);
        }

        private void StopWallJumping()
        {
          _wallJumping = false;
        }
        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashPressed;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }

    public enum DashState
    {
      Ready,
      Dashing,
      Cooldown
    }

    public enum WallSlideState
    {
      NotOnWall,
      Sliding,
      Jumping,
    }
}