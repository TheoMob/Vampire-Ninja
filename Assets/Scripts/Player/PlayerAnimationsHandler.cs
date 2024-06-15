using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IPlayerState;
using static IPlayerState.PlayerStateController;
using TarodevController;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using System;

public class PlayerAnimationsHandler : MonoBehaviour
{
  private const string PLAYER_IDLE = "PlayerIdle";
  private const string PLAYER_WALK = "PlayerWalk";
  private const string PLAYER_JUMP_UP = "PlayerJumpUp";
  private const string PLAYER_JUMP_DOWN = "PlayerJumpDown";
  private const string PLAYER_JUMP_TRANSITION = "PlayerJumpTransition";
  //private const string PLAYER_DASH_HORIZONTAL = "PlayerDashHorizontal";
  private const string PLAYER_DASH_HORIZONTAL = "PlayerDashHorizontal2";
  private const string PLAYER_DASH_VERTICAL = "PlayerDashVertical";
  private const string PLAYER_WALL_SLIDE = "PlayerWallSlide";
  private const string PLAYER_DEFEATED = "PlayerDefeated";
  private const string PLAYER_CROUCH = "PlayerCrouch";
  private const string PLAYER_ATTACK_1 = "PlayerAttack1";
  private PlayerStateController _stateController;
  private Animator playerAnimator;
  private SpriteRenderer playerSR;

  //Dash Effects and materials
  [SerializeField] private bool useDashEffects = false;
  [ShowIf("useDashEffects")] [SerializeField] private GameObject dashAfterImageObject;
  [ShowIf("useDashEffects")] [SerializeField] private Material dashHorizontalMaterial;
  [ShowIf("useDashEffects")] [SerializeField] private Material dashVerticalMaterial;
  private ParticleSystemRenderer dashParticleRendered;
  void Start()
  {
    playerAnimator = GetComponent<Animator>();
    playerSR = GetComponent<SpriteRenderer>();
    _stateController = GetComponent<PlayerStateController>();

    if (useDashEffects)
      dashParticleRendered = dashAfterImageObject.GetComponent<ParticleSystemRenderer>();
  }

  void Update()
  {
    if (_stateController.GetCurrentState() == PlayerState.Defeated)
    {
      playerAnimator.Play(PLAYER_DEFEATED);
      return;
    }

    AnimationChooser();

    if (_stateController.canPlayerMove())
      InvertSprite();

    if (useDashEffects)
      HandleDashParticles();

    AnimateDashAvailability(); // so the player has a visual cue to wheter the dash is available or not
  }

  private void InvertSprite()
  {
    float playerInput = Input.GetAxisRaw("Horizontal");
    if (playerInput == 0)
      return;

    playerSR.flipX = playerInput < 0 ? true : false;
  }

  private void AnimateDashAvailability()
  {
    if (_stateController.getDashState() == DashState.Cooldown && !_stateController.isDashReset())
      playerSR.color = Color.red;
    else
      playerSR.color = Color.white;
  }

  private string previousAnimation = PLAYER_IDLE;
  private void AnimationChooser()
  {
    PlayerState currentState = _stateController.GetCurrentState();
    JumpState jumpState = _stateController.getJumpState();
    string currentAnimation = PLAYER_IDLE;

    switch(currentState)
    {
      case PlayerState.CantMove:
        currentAnimation = previousAnimation;
      break;
      
      case PlayerState.Defeated:
        currentAnimation = PLAYER_DEFEATED;
      break;

      case PlayerState.Attacking:
        currentAnimation = PLAYER_ATTACK_1;
      break;

      case PlayerState.WallSliding:
        currentAnimation = PLAYER_WALL_SLIDE;
      break;

      case PlayerState.Dashing:
        currentAnimation = _stateController.IsDashVertical()? PLAYER_DASH_VERTICAL : PLAYER_DASH_HORIZONTAL;
      break;

      case PlayerState.Jumping:
        switch(jumpState)
        {
          case JumpState.Ascending:
            currentAnimation = PLAYER_JUMP_UP;
          break;
          case JumpState.Transition:
            currentAnimation = PLAYER_JUMP_TRANSITION;
          break;
          case JumpState.Idle:
            currentAnimation = PLAYER_JUMP_DOWN;
          break;
        }
      break;

      case PlayerState.WallJumping:
        currentAnimation = PLAYER_JUMP_UP;
      break;

      case PlayerState.Crouching:
        currentAnimation = PLAYER_CROUCH;
      break;

      case PlayerState.Running:
        currentAnimation = PLAYER_WALK;
      break;

      case PlayerState.Idle:
        currentAnimation = PLAYER_IDLE;
      break;
    }


    playerAnimator.Play(currentAnimation);
    playerAnimator.speed = currentState == PlayerState.CantMove ? 0 : 1;
    previousAnimation = currentAnimation;
  }

  #region Effects

  public void SendDashAnimation()
  {
    if (_stateController.GetCurrentState() != PlayerState.Dashing || !useDashEffects)
      return;

    string currentAnimation = _stateController.IsDashVertical()? PLAYER_DASH_VERTICAL : PLAYER_DASH_HORIZONTAL;

    playerAnimator.Play(PLAYER_IDLE);
    playerAnimator.Play(currentAnimation);
    HandleDashParticles();
  }
  public void HandleDashParticles()
  {
    bool isDashing = _stateController.GetCurrentState() == PlayerState.Dashing;
    dashAfterImageObject.SetActive(isDashing);

    if (!isDashing)
      return;

    Material dashMaterial = _stateController.IsDashVertical() ? dashVerticalMaterial : dashHorizontalMaterial;

    if (dashParticleRendered.material != dashMaterial)
      dashParticleRendered.material = dashMaterial;

    dashParticleRendered.flip = playerSR.flipX ? Vector2.right : Vector2.zero;
  }
  #endregion
}
