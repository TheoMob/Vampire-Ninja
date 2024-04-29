using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TarodevController;
using UnityEngine;

public class PlayerAnimationsHandler : MonoBehaviour
{
  private const string PLAYER_IDLE = "PlayerIdle";
  private const string PLAYER_WALK = "PlayerWalk";
  private const string PLAYER_JUMP_UP = "PlayerJumpUp";
  private const string PLAYER_JUMP_DOWN = "PlayerJumpDown";
  private const string PLAYER_JUMP_TRANSITION = "PlayerJumpTransition";
  private const string PLAYER_DASH = "PlayerDash";
  private const string PLAYER_WALL_SLIDE = "PlayerWallSlide";
  private const string PLAYER_DEFEATED = "PlayerDefeated";
  private const string PLAYER_CROUCH = "PlayerCrouch";
  [SerializeField] private ScriptableStats _stats;
  [SerializeField] private GameObject dashAfterImageObject;
  private ParticleSystemRenderer dashParticleRendered;
  private PlayerController _playerController;
  private CapsuleCollider2D playerFeetCollider;
  private Rigidbody2D playerRB;
  private Animator playerAnimator;

  private bool isGrounded = false;
  private bool playerIsDefeated = false;
  void Start()
  {
    playerFeetCollider = GetComponent<CapsuleCollider2D>();
    playerAnimator = GetComponent<Animator>();
    playerRB = GetComponent<Rigidbody2D>();
    _playerController = GetComponent<PlayerController>();
    dashParticleRendered = dashAfterImageObject? dashAfterImageObject.GetComponent<ParticleSystemRenderer>() : null;

    GameManager.PlayerDefeatedEvent += PlayDefeatedAnimation;

    playerIsDefeated = false;
  }

  void OnDestroy()
  {
    GameManager.PlayerDefeatedEvent -= PlayDefeatedAnimation;
  }

  // Update is called once per frame
  void Update()
  {
    if (playerIsDefeated)
      return;

    CheckCollisions();
    AnimationChooser();

    if (_playerController._playerControl)
      InvertSprite();

    if (dashParticleRendered)
      HandleDashParticles();
  }

  private void CheckCollisions()
  {
    isGrounded = Physics2D.CapsuleCast(playerFeetCollider.bounds.center, playerFeetCollider.size, playerFeetCollider.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
  }

  private void InvertSprite()
  {
    float scale = Input.GetAxisRaw("Horizontal");

    if (scale == 0)
    {
      return;
    }

    transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * scale, transform.localScale.y);
  }

  private void AnimationChooser()
  {
    if (_playerController._wallSliding)
    {
      playerAnimator.Play(PLAYER_WALL_SLIDE);
      return;
    }

    if (_playerController.dashState == DashState.Dashing)
    {
      playerAnimator.Play(PLAYER_DASH);
      return;
    }

    if (!isGrounded)
    {
      switch(playerRB.velocity.y)
      {
        case > 0:
          playerAnimator.Play(PLAYER_JUMP_UP);
        break;
        case < 0:
          playerAnimator.Play(PLAYER_JUMP_DOWN);
        break;
        case 0:
          playerAnimator.Play(PLAYER_JUMP_TRANSITION);
        break;
      }

      return;
    }

    if (_playerController._crouching)
    {
      playerAnimator.Play(PLAYER_CROUCH);
      return;
    }

    if (playerRB.velocity.x != 0)
      playerAnimator.Play(PLAYER_WALK);
    else
      playerAnimator.Play(PLAYER_IDLE);
  }

  private void PlayDefeatedAnimation()
  {
    playerIsDefeated = true;
    playerAnimator.Play(PLAYER_DEFEATED);
  }

  private void HandleDashParticles()
  {
    dashAfterImageObject.SetActive(_playerController.dashState == DashState.Dashing);

    float scale = Input.GetAxisRaw("Horizontal");
    if (scale == 1)
      dashParticleRendered.flip = Vector2.zero;
    else if (scale == -1)
      dashParticleRendered.flip = Vector2.right;
  }
}
