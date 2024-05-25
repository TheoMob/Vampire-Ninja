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
  private const string PLAYER_DASH_HORIZONTAL = "PlayerDashHorizontal";
  private const string PLAYER_DASH_VERTICAL = "PlayerDashVertical";
  private const string PLAYER_WALL_SLIDE = "PlayerWallSlide";
  private const string PLAYER_DEFEATED = "PlayerDefeated";
  private const string PLAYER_CROUCH = "PlayerCrouch";
  private const string PLAYER_ATTACK_1 = "PlayerAttack1";
  private PlayerStateController _stateController;
  private Rigidbody2D playerRB;
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
    playerRB = GetComponent<Rigidbody2D>();
    playerSR = GetComponent<SpriteRenderer>();
    _stateController = GetComponent<PlayerStateController>();

    if (useDashEffects)
      dashParticleRendered = dashAfterImageObject.GetComponent<ParticleSystemRenderer>();

    GameManager.PlayerDefeatedEvent += PlayDefeatedAnimation;
  }

  void OnDestroy()
  {
    GameManager.PlayerDefeatedEvent -= PlayDefeatedAnimation;
  }
  void Update()
  {
    if (_stateController.GetCurrentState() == PlayerState.Defeated)
      return;

    AnimationChooser();

    if (_stateController.canPlayerMove())
      InvertSprite();

    if (useDashEffects)
      HandleDashParticles();

    AnimateDashAvailability();
  }

  private void InvertSprite()
  {
    float scale = Input.GetAxisRaw("Horizontal");

    if (scale == 0)
      return;

    transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * scale, transform.localScale.y);
  }

  private void AnimateDashAvailability()
  {
    if (_stateController.getDashState() == DashState.Cooldown)
      playerSR.color = Color.red;
    else
      playerSR.color = Color.white;
  }

  private void AnimationChooser()
  {
    PlayerState currentState = _stateController.GetCurrentState();
    AnimatorStateInfo currentAnimationClip = playerAnimator.GetCurrentAnimatorStateInfo(0);

    if (currentState == PlayerState.Attacking)
    {
      playerAnimator.Play(PLAYER_ATTACK_1);
      return;
    }

    if (currentState == PlayerState.WallSliding)
    {
      playerAnimator.Play(PLAYER_WALL_SLIDE);
      return;
    }

    if (currentState == PlayerState.Dashing)
    {
      if (_stateController.IsDashVertical())
        playerAnimator.Play(PLAYER_DASH_VERTICAL);
      else
        playerAnimator.Play(PLAYER_DASH_HORIZONTAL);

      return;
    }

    if (currentState == PlayerState.Jumping || currentState == PlayerState.WallJumping)
    {
      bool correctAnimationTransition = currentAnimationClip.IsName(PLAYER_JUMP_UP) || currentAnimationClip.IsName(PLAYER_JUMP_TRANSITION);

      if (correctAnimationTransition && playerRB.velocity.y <= 0.1f)
      {
        playerAnimator.Play(PLAYER_JUMP_TRANSITION);
        return;
      }

      if (playerRB.velocity.y > 0)
      {
        playerAnimator.Play(PLAYER_JUMP_UP);
        return;
      }

      if (playerRB.velocity.y <= 0)
      {
        playerAnimator.Play(PLAYER_JUMP_DOWN);
        return;
      }
    }

    if (currentState == PlayerState.Crouching)
    {
      playerAnimator.Play(PLAYER_CROUCH);
      return;
    }

    if (playerRB.velocity.x != 0) // test if the player is not moving and also not pressing to move
    {
      playerAnimator.Play(PLAYER_WALK);
      return;
    }

    if (playerRB.velocity.x == 0 && _stateController.GetFrameInput().Move.x == 0)
      playerAnimator.Play(PLAYER_IDLE);
  }

  private void PlayDefeatedAnimation()
  {
    playerAnimator.Play(PLAYER_DEFEATED);
  }

  #region Effects

  private void HandleDashParticles()
  {
    bool isDashing = _stateController.GetCurrentState() == PlayerState.Dashing;
    dashAfterImageObject.SetActive(isDashing);

    if (!isDashing)
      return;

    Material dashMaterial = _stateController.IsDashVertical() ? dashVerticalMaterial : dashHorizontalMaterial;

    if (dashParticleRendered.material != dashMaterial)
      dashParticleRendered.material = dashMaterial;

    dashParticleRendered.flip = transform.localScale.x < 0 ? Vector2.right : Vector2.zero;
  }
  #endregion
}
