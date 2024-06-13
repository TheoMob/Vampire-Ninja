using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

namespace IPlayerState
{
  public class PlayerStateController : MonoBehaviour
  {
    public PlayerState playerState;
    private PlayerController _playerMovementController;
    public bool isDefeated = false;
    public bool cantMove = false;
    private CapsuleCollider2D _col;
    private Rigidbody2D _rb;
    private SpriteRenderer _spr;
    private GameManager _gameManager;

    [Header("Player Respawn Delay")]
    [SerializeField] private float respawnDelay;

    private void Awake()
    {
      _playerMovementController = GetComponent<PlayerController>();
      _col = GetComponent<CapsuleCollider2D>();
      _rb = GetComponent<Rigidbody2D>();
      _spr = GetComponent<SpriteRenderer>();

      _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

      originalHitBoxOffset = _col.offset;
      originalHitBoxSize = _col.size;
    }

    private void FixedUpdate()
    {
      playerState = RefreshState();

      AdjustHitBoxWhenCrouched();
    }

    public PlayerState GetCurrentState()
    {
      return playerState;
    }

    public void SetState(PlayerState newState)
    {
      // add protections and verifications
      playerState = newState;
    }
    
    public PlayerState RefreshState()
    {
      bool grounded = _playerMovementController._grounded;
      bool pressingDown = _playerMovementController._frameInput.Move.y < 0;
      bool dashing = _playerMovementController.dashState == DashState.Dashing;
      bool wallJumping = _playerMovementController.wallJumping;
      bool isTouchingWall = CheckIfItsWalled();
      Vector2 frameVelocity = _playerMovementController._frameVelocity;
      FrameInput frameInput = _playerMovementController._frameInput;

      if (isDefeated)
        return PlayerState.Defeated;

      if (cantMove)
        return PlayerState.CantMove;

      if (dashing)
        return PlayerState.Dashing;

      if (!grounded && wallJumping)
        return PlayerState.WallJumping;

      if (isTouchingWall && !grounded && frameInput.Move.x != 0)
        return PlayerState.WallSliding;

      if (grounded && pressingDown)
        return PlayerState.Crouching;

      if (grounded && !pressingDown && frameInput.Move.x != 0)
        return PlayerState.Running;

      if (!grounded && !wallJumping)
        return PlayerState.Jumping;

      return PlayerState.Idle;
    }

    public bool canPlayerMove()
    {
      return playerState != PlayerState.Dashing && playerState != PlayerState.Attacking && playerState != PlayerState.Defeated && playerState != PlayerState.WallSliding && playerState != PlayerState.CantMove;
    }

    public FrameInput GetFrameInput()
    {
      return _playerMovementController._frameInput;
    }

    #region Dash and wallJump Checkers

    public bool IsDashVertical()
    {
      return _playerMovementController.isDashVertical;
    }

    public DashState getDashState()
    {
      return _playerMovementController.dashState;
    }

    public bool isDashReset()
    {
      return _playerMovementController._dashReset;
    }

    public JumpState getJumpState()
    {
      return _playerMovementController.jumpState;
    }
    private bool CheckIfItsWalled()
    {
      float direction = _spr.flipX ? -1 : 1;
      Vector2 lookingDirection = Vector2.right * direction * (_col.size.x / 1f);

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
      
      //return middleCheck && bottomCheck && topCheck;
      return middleCheck && bottomCheck;
    }
    #endregion

    #region Crouched Hitbox
    private Vector2 originalHitBoxOffset;
    private Vector2 originalHitBoxSize;
    private Vector2 crouchedHitBoxOffset = new Vector2(0.02627944f, -0.3927892f);
    private Vector2 crouchedHitBoxSize = new Vector2(0.8681107f, 1.409917f);
    private void AdjustHitBoxWhenCrouched()
    {
      if (playerState == PlayerState.Crouching)
      {
        _col.size = crouchedHitBoxSize;
        _col.offset = crouchedHitBoxOffset;
      }
      else
      {
        _col.size = originalHitBoxSize;
        _col.offset = originalHitBoxOffset;
      }
    }
    #endregion

    #region PlayerDefeated
    public void OnPlayerDefeated()
    {
        _col.enabled = false;
        //_rb.isKinematic = true;
        _rb.velocity = Vector2.zero;
        _playerMovementController._frameVelocity = Vector2.zero;
        _playerMovementController.jumpState = JumpState.Idle;
        isDefeated = true;
        _gameManager.ReturnToCheckPoint(respawnDelay);  
        Invoke(nameof(ReturnPlayerToLife), respawnDelay);
    }

    private void ReturnPlayerToLife()
    {
      	_col.enabled = true;
        //_rb.isKinematic = false;
        isDefeated = false;
    }
    #endregion

    public enum PlayerState
    {
      Idle,
      CantMove,
      Running,
      Crouching,
      Attacking,
      Dashing,
      WallSliding,
      WallJumping,
      Jumping,
      Defeated,
    }
  }
}
