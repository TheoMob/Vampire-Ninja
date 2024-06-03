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
    private PlayerCombatController _playerCombatController;
    public bool isDefeated = false;
    private CapsuleCollider2D _col;
    private Rigidbody2D _rb;

    private void Awake()
    {
      _playerMovementController = GetComponent<PlayerController>();
      _playerCombatController = GetComponent<PlayerCombatController>();
      _col = GetComponent<CapsuleCollider2D>();
      _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
      playerState = RefreshState();

      if (isDefeated)
        _rb.velocity = Vector2.zero;
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
      bool isAttacking = _playerCombatController.isAttacking;
      Vector2 frameVelocity = _playerMovementController._frameVelocity;
      FrameInput frameInput = _playerMovementController._frameInput;

      if (isDefeated)
        return PlayerState.Defeated;

      if (isAttacking)
        return PlayerState.Attacking;

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
      if (playerState == PlayerState.Dashing || playerState == PlayerState.Attacking || playerState == PlayerState.Defeated || playerState == PlayerState.WallSliding)
        return false;

      return true;
    }

    public FrameInput GetFrameInput()
    {
      return _playerMovementController._frameInput;
    }

    public bool IsDashVertical()
    {
      return _playerMovementController.isDashVertical;
    }

    public DashState getDashState()
    {
      return _playerMovementController.dashState;
    }

    public JumpState getJumpState()
    {
      return _playerMovementController.jumpState;
    }


    private bool CheckIfItsWalled()
    {
      Vector2 lookingDirection = Vector2.right * Math.Sign(transform.localScale.x) * (_col.size.x / 1f);
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
      
      return middleCheck && bottomCheck && topCheck;
    }

    public enum PlayerState
    {
      Idle,
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
