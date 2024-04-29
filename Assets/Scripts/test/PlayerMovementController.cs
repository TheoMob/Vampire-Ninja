// using System;
// using System.Collections;
// using Unity.Mathematics;
// using Unity.VisualScripting;
// using UnityEngine;

// namespace TarodevController
// {
//     /// <summary>
//     /// Hey!
//     /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
//     /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
//     /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
//     /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
//     /// </summary>
//     [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
//     public class PlayerController : MonoBehaviour, IPlayerController
//     {
//         [SerializeField] private ScriptableStats _stats;
//         private Rigidbody2D _rb;
//         private CapsuleCollider2D _col;
//         private FrameInput _frameInput;
//         private Vector2 _frameVelocity;
//         private bool _cachedQueryStartInColliders;

//         #region Interface

//         public Vector2 FrameInput => _frameInput.Move;
//         public event Action<bool, float> GroundedChanged;
//         public event Action Jumped;

//         #endregion

//         private float _time;
//         public bool _playerControl;

//         private void Awake()
//         {
//           _playerControl = true;
//           _rb = GetComponent<Rigidbody2D>();
//           _col = GetComponent<CapsuleCollider2D>();

//           _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
//           _gravityScale = _stats.FallAcceleration;
//         }

//         private void Update()
//         {
//             _time += Time.deltaTime;
//             GatherInput();
//         }

//         private void GatherInput()
//         {
//             _frameInput = new FrameInput
//             {
//                 JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
//                 JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
//                 DashPressed = Input.GetButtonDown("Fire3"),
//                 Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
//             };

//             if (_stats.SnapInput)
//             {
//                 _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
//                 _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
//             }

//             if (_frameInput.JumpDown)
//             {
//                 _jumpToConsume = true;
//                 _timeJumpWasPressed = _time;
//             }

//             if (_frameInput.DashPressed)
//             {
//               _timeDashWasPressed = _time;
//               _dashPressed = true;
//             }
//         }

//         private void FixedUpdate()
//         {
//             CheckCollisions();

//             if (_playerControl)
//             {
//               HandleJump();
//               HandleGravity();
//               HandleDirection();
//             }
//             HandleDash();

//             ApplyMovement();
//         }

//         #region Collisions

//         private float _frameLeftGrounded = float.MinValue;
//         private bool _grounded;

//         private void CheckCollisions()
//         {
//             Physics2D.queriesStartInColliders = false;

//             // Ground and Ceiling
//             bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, LayerMask.GetMask("Ground"));
//             bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, LayerMask.GetMask("Ground"));

//             // Hit a Ceiling
//             if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

//             if (groundHit) _groundHitAfterDash = true;

//             // Landed on the Ground
//             if (!_grounded && groundHit)
//             {
//                 _grounded = true;
//                 _coyoteUsable = true;
//                 _bufferedJumpUsable = true;
//                 _endedJumpEarly = false;
//                 GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
//             }
//             // Left the Ground
//             else if (_grounded && !groundHit)
//             {
//                 _grounded = false;
//                 _frameLeftGrounded = _time;
//                 GroundedChanged?.Invoke(false, 0);
//             }

//             Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
//         }

//         #endregion


//         #region Jumping

//         private bool _jumpToConsume;
//         private bool _bufferedJumpUsable;
//         private bool _endedJumpEarly;
//         private bool _coyoteUsable;
//         private float _timeJumpWasPressed;

//         private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
//         private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

//         private void HandleJump()
//         {
//             if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

//             if (!_jumpToConsume && !HasBufferedJump) return;

//             if (_grounded || CanUseCoyote) ExecuteJump();

//             _jumpToConsume = false;
//         }

//         private void ExecuteJump()
//         {
//             _endedJumpEarly = false;
//             _timeJumpWasPressed = 0;
//             _bufferedJumpUsable = false;
//             _coyoteUsable = false;
//             _frameVelocity.y = _stats.JumpPower;
//             Jumped?.Invoke();
//         }

//         #endregion

//         #region Horizontal

//         private void HandleDirection()
//         {
//           if (_frameInput.Move.x == 0)
//           {
//             var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;

//             _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
//           }
//           else
//           {
//             _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
//           }
//         }

//         #endregion

//         #region Gravity

//         private float _gravityScale;
//         public GravityState gravityState;

//         private void HandleGravity()
//         {
//           if (_grounded && _frameVelocity.y <= 0f) // means it's grounded
//           {
//             _frameVelocity.y = _stats.GroundingForce;
//             return;
//           }

//           var fallSpeed = _gravityScale;
//           if (_endedJumpEarly || _frameVelocity.y > 0)
//             fallSpeed *= _stats.JumpEndEarlyGravityModifier;

//           _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, fallSpeed * Time.fixedDeltaTime);
//         }

//         #endregion

//         #region Dash

//         public DashState dashState;
//         private bool _groundHitAfterDash = true;
//         private float _lastDashTime = 0;
//         public bool _dashReset = false;
//         private bool _dashPressed = false;
//         private float _timeDashWasPressed;

//         private void HandleDash()
//         {
//           switch(dashState)
//           {
//             case DashState.Ready:
//               float dashPressedTime = _timeDashWasPressed + _stats.DashBuffer;
//               if (dashPressedTime < Time.time)
//                 _dashPressed = false;

//               if (_dashPressed)
//               {
//                 _dashPressed = false;
//                 _playerControl = false;
//                 _dashReset = false;
//                 _groundHitAfterDash = false;
//                 _lastDashTime = Time.time;
//                 Time.timeScale = 1 - _stats.DashSlowOnTime;
//                 StartCoroutine(DashDelayedExecution());
//               }
//             break;

//             case DashState.Dashing:
//               bool dashIsOver = _lastDashTime + _stats.DashDuration <= Time.time;
//               if (dashIsOver)
//               {
//                 dashState = DashState.Cooldown;
//                 _playerControl = true;
//                 StartCoroutine(DelayToReturnGravity());
//               }
//             break;

//             case DashState.Cooldown:
//              if (_dashReset || (_groundHitAfterDash && _lastDashTime + _stats.DashCooldown <= Time.time))
//               dashState = DashState.Ready;
//             break;
//           }
//         }

//         IEnumerator DelayToReturnGravity()
//         {
//           yield return new WaitForSeconds(_stats.DashGravityReturnDelay * Time.timeScale);
//           _gravityScale = _stats.FallAcceleration;
//         }
//         IEnumerator DashDelayedExecution()
//         {
//           yield return new WaitForSeconds(_stats.DashInputDelay * Time.timeScale);

//           CinemachineShake.Instance.ShakeCamera(_stats.DashShakeIntensity, _stats.DashShakeDuration);
//           _gravityScale = 0;
//           Time.timeScale = 1f;
//           dashState = DashState.Dashing;

//           switch(_frameInput.Move)
//           {
//             case var _ when _frameInput.Move == Vector2.zero: // horizontal dashes without pressing any directions
//               _frameVelocity.x += Math.Sign(transform.localScale.x) * _stats.DashSpeed;
//               _frameVelocity.y = 0;
//             break;

//             case var _ when _frameInput.Move.x != 0 && _frameInput.Move.y == 0: // horizontal dashes pressing left or right
//               _frameVelocity.x += _frameInput.Move.x * _stats.DashSpeed;
//               _frameVelocity.y = 0;
//             break;

//             case var _ when _frameInput.Move.x == 0 && _frameInput.Move.y != 0: // vertical dashes
//               _frameVelocity.x = 0;
//               _frameVelocity.y += _frameInput.Move.y * _stats.DashSpeed;
//             break;

//             case var _ when _frameInput.Move.x != 0 && _frameInput.Move.y != 0: // diagonal dashes
//               _frameVelocity.x += _frameInput.Move.x * _stats.DashSpeed;
//               _frameVelocity.y += _frameInput.Move.y * _stats.DashSpeed;
//             break;
//           }
//         }
//         #endregion

//         private void ApplyMovement() => _rb.velocity = _frameVelocity;

// #if UNITY_EDITOR
//         private void OnValidate()
//         {
//             if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
//         }
// #endif
//     }

//     public struct FrameInput
//     {
//         public bool JumpDown;
//         public bool JumpHeld;
//         public bool DashPressed;
//         public Vector2 Move;
//     }

//     public interface IPlayerController
//     {
//         public event Action<bool, float> GroundedChanged;

//         public event Action Jumped;
//         public Vector2 FrameInput { get; }
//     }

//     public enum DashState
//     {
//       Ready,
//       Dashing,
//       Cooldown
//     }

//     public enum GravityState
//     {
//       Grounded,
//       Falling,
//       Jumping
//     }
// }