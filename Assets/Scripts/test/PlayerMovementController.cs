using System;
using System.Collections;
using IPlayerState;
using MovementStatsController;
using TarodevController;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static IPlayerState.PlayerStateController;
public class PlayerMovementController : MonoBehaviour
{
    // external scripts
    [SerializeField] private ScriptableStats _stats;
    private PlayerStateController _stateController;
    // components
    private Rigidbody2D playerRb;
    private CapsuleCollider2D bodyCollider;

    //Input
    private NewFrameInput frameInput;
    private Vector2 frameVelocity;

    private float frameTime;
    private PlayerState currentState;

    private void Awake()
    {
        _stateController = GetComponent<PlayerStateController>();
        playerRb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        _gravityScale = _stats.FallAcceleration;
    }

    private void Update()
    {
        currentState = _stateController.GetCurrentState();
        frameTime += Time.deltaTime;
        GatherInput();
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleGravity();
        HandleJump();
        HandleDirection();
        HandleCrouch();
        
        ApplyMovement();
    }

    private void ApplyMovement() => playerRb.velocity = frameVelocity;

    private void GatherInput()
    {
        frameInput = new NewFrameInput
        {
            JumpPressed = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump"),
            DashPressed = Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.K),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
        };

        if (_stats.SnapInput)
        {
            frameInput.Move.x = Mathf.Abs(frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.x);
            frameInput.Move.y = Mathf.Abs(frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.y);
        }

        if (frameInput.JumpPressed)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = frameTime;
        }

        // if (frameInput.DashPressed)
        // {
        //     _timeDashWasPressed = frameTime;
        //     _dashPressed = true;
        // }
    }

    public NewFrameInput returnFrameInput()
    {
        return frameInput;
    }

    #region Collisions

        public bool _grounded;
        private float _frameLeftGrounded = float.MinValue;
        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, Vector2.down, _stats.GrounderDistance, LayerMask.GetMask("Ground") | LayerMask.GetMask("OneWayPlatform"));
            bool ceilingHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, Vector2.up, _stats.GrounderDistance, LayerMask.GetMask("Ground"));

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                //_dashReset = true;
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
               //_dashPressed = false; // so it doesnt dash after landing if the dash was pressed in the air
            }
            else if (_grounded && !groundHit) // left the ground
            {
                _grounded = false;
                _frameLeftGrounded = frameTime;
            }
        }
    #endregion


    #region Jumping

        [SerializeField] private float jumpAscentionDuration = 0.2f;
        [SerializeField] private float jumpTransitionDuration = 0.2f;
        private JumpState jumpState = JumpState.Notjumping;
        public bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        [SerializeField] private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && frameTime < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && frameTime < _frameLeftGrounded + _stats.CoyoteTime && _stateController.GetCurrentState() != PlayerState.Crouching;
        private void HandleJump()
        {
          _endedJumpEarly = ShouldEndJumpEarly();

          if (!_jumpToConsume && !HasBufferedJump)
            return;

          if (_grounded || CanUseCoyote) ExecuteJump();

          if (_stateController.GetCurrentState() == PlayerState.WallSliding || wallJumpBufferTimer > 0f)
            return;

          _jumpToConsume = false;
        }

        private bool ShouldEndJumpEarly()
        {
          bool improperState = currentState == PlayerState.WallSliding || currentState == PlayerState.WallJumping ? true : false;

          return !_endedJumpEarly && !_grounded && !frameInput.JumpHeld && playerRb.velocity.y > 0
              && !improperState && wallJumpBufferTimer < 0f;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            frameVelocity.y = _stats.JumpPower;
            StartCoroutine(handleJumpState()); 
        }

        private IEnumerator handleJumpState()
        {
            jumpState = JumpState.Ascending;
            yield return new WaitForSeconds(jumpAscentionDuration);
            StartCoroutine(handleJumpTransition());
        }

        private IEnumerator handleJumpTransition()
        {
            jumpState = JumpState.Transition;
            yield return new WaitForSeconds(jumpTransitionDuration);
            jumpState = JumpState.Notjumping;
        }

    #endregion

    #region Gravity
    private float _gravityScale;

    private void HandleGravity()
    {
        if (_grounded && frameVelocity.y <= 0f) // means it's grounded
        {
            frameVelocity.y = _stats.GroundingForce;
            return;
        }

        if (_endedJumpEarly)
        {
            StopCoroutine(handleJumpState());
            StopCoroutine(handleJumpTransition());
            StartCoroutine(handleJumpTransition());
        }

        if (jumpState == JumpState.Ascending)
            return;
        
        if (jumpState == JumpState.Transition)
        {
            frameVelocity.y = 0;
            return;
        }

        frameVelocity.y = -_stats.MaxFallSpeed;
    }
    #endregion

    #region Crouch
        private void HandleCrouch()
        {
          if (!_grounded || frameVelocity.y >= 0)
          {
            return;
          }

          if(currentState == PlayerState.Crouching)
          {
            frameVelocity.x = 0;
          }
        }

    #endregion
    #region Horizontal

        private void HandleDirection()
        {
          if (frameInput.Move.x == 0)
          {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;

            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
          }
          else
          {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
          }
        }

    #endregion

    private float wallJumpBufferTimer = -1f;

    private enum JumpState
    {
        Notjumping,
        Ascending,
        Transition,
    }
    public struct NewFrameInput
    {
        public Vector2 Move;
        public bool JumpPressed;
        public bool JumpHeld;
        public bool DashPressed;
    }
}