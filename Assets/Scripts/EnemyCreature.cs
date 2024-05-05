using System.Collections;
using System.Collections.Generic;
using TarodevController;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCreature : MonoBehaviour
{
  private PlayerController _playerController;
  private PlayerCombatController _playerCombat;
  private GameManager _gameManager;
  private SpriteRenderer sprRenderer;
  private EnemyAI _enemyAI;
  private CircleCollider2D attackCollider;
  private Rigidbody2D rb;
  private Animator anim;

  [SerializeField] private float delayToReappear = 2f;
  [SerializeField] private float slowOnKill = 0.3f;
  [SerializeField] private float slowDuration = 0.5f;

  [Header("Camera shake on kill")]
  [SerializeField] private float shakeIntensity = 5f;
  [SerializeField] private float duration = 0.1f;

  [Header("Attack variables")]
  [SerializeField] private float attackSpeed = 5f;
  [SerializeField] private float attackDuration = 2f;
  [SerializeField] private float attackPreparationTime = 0.5f;
  [SerializeField] private float attackCooldown = 2f;
  [HideInInspector] public bool isAttackOnCooldown;

  private const string IDLE_ANIMATION = "EnemyIdle";
  private const string DEATH_ANIMATION = "EnemyDeath";
  private const string REAPPEAR_ANIMATION = "EnemyReappear";
  private const string ATTACK_PREPARATION_ANIMATION = "EnemyAttack";
  private const string ATTACK_ANIMATION = "EnemyAttack";
  [HideInInspector] public bool isAttacking = false;
  private float deathAnimDuration;
  private bool isCreatureDead = false;

  private void Awake()
  {
    _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    _playerCombat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombatController>();
    _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    _enemyAI = GetComponent<EnemyAI>();

    foreach(Transform child in transform)
    {
      if (child.CompareTag("Body"))
      {
        sprRenderer = child.GetComponent<SpriteRenderer>();
        rb = child.GetComponent<Rigidbody2D>();
        anim = child.GetComponent<Animator>();
        attackCollider = child.GetChild(0).GetComponent<CircleCollider2D>();
        break;
      }
    }

    UpdateAnimClipTimes();
  }

  private void Update()
  {
    InvertSprite();
  }

  private void InvertSprite()
  {
    // float scale = -math.sign(rb.velocity.x);

    // if (math.abs(rb.velocity.x) < .2f)
    //   return;

    // transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * scale, transform.localScale.y);
  }

  private void KillCreature()
  {
    _playerController._dashReset = true;
    _gameManager.ShakeCamera(shakeIntensity, duration);
    _gameManager.SlowTime(slowOnKill, slowDuration);

    anim.Play(DEATH_ANIMATION);
    attackCollider.enabled = false;
    _enemyAI.isCreatureDead = true;
    isCreatureDead = true;

    Invoke(nameof(DeactivateSpriteRenderer), deathAnimDuration);
    Invoke(nameof(Reappear), delayToReappear);
  }
  private void DeactivateSpriteRenderer()
  {
    sprRenderer.enabled = false;
  }

  private void DamagePlayer()
  {
    _playerCombat.PlayerLosesHealth(1);
  }

  private void Reappear()
  {
    anim.Play(REAPPEAR_ANIMATION);
    sprRenderer.enabled = true;
    attackCollider.enabled = true;
    _enemyAI.isCreatureDead = false;
    isCreatureDead = false;
  }

  public void HandleAttack()
  {
    if (isAttacking || isAttackOnCooldown || isCreatureDead)
      return;

    isAttacking = true;
    sprRenderer.color = Color.yellow;
    anim.Play(ATTACK_PREPARATION_ANIMATION);
    Invoke(nameof(Attack), attackPreparationTime);
    Invoke(nameof(AttackEnd), attackDuration + attackPreparationTime);
    StartCoroutine(setAttackCooldown());
  }

  IEnumerator setAttackCooldown()
  {
    isAttackOnCooldown = true;
    yield return new WaitForSeconds(attackCooldown);
    isAttackOnCooldown = false;
  }

  private void Attack()
  {
    if (isCreatureDead)
      return;

    sprRenderer.color = Color.red;
    rb.velocity = new Vector2(attackSpeed * -math.sign(transform.localScale.x), 0);
  }

  private void AttackEnd()
  {
    isAttacking = false;
    rb.velocity = Vector2.zero;
    sprRenderer.color = Color.white;
    anim.Play(IDLE_ANIMATION);
  }

  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.tag != "Player")
      return;

    if (_playerController.dashState == DashState.Dashing)
      KillCreature();
    else
      DamagePlayer();
  }

  private void UpdateAnimClipTimes()
  {
    AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
    foreach(AnimationClip clip in clips)
    {
      switch(clip.name)
      {
        case DEATH_ANIMATION:
          deathAnimDuration = clip.length;
          break;
      }
    }
  }
}
