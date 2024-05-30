using System.Collections;
using TarodevController;
using Unity.Mathematics;
using UnityEngine;
using NaughtyAttributes;
using IPlayerState;
using static IPlayerState.PlayerStateController;

public class EnemyCreature : MonoBehaviour
{
  protected PlayerStateController _playerStateController;
  protected PlayerCombatController _playerCombat;
  protected GameManager _gameManager;
  protected SpriteRenderer sprRenderer;
  protected Rigidbody2D rb;
  protected Animator anim;
  protected Collider2D bodyHitboxCollider;

  [SerializeField] protected bool passiveMob = true;

  [SerializeField] protected bool reappear = false;
  [ShowIf("reappear")] [SerializeField] protected float delayToReappear = 2f;

  // attack variables
  [HideIf("passiveMob")] [SerializeField] protected float attackSpeed = 5f;
  [HideIf("passiveMob")] [SerializeField] protected float attackDuration = 2f;
  [HideIf("passiveMob")] [SerializeField] protected float attackPreparationTime = 0.5f;
  [HideIf("passiveMob")] [SerializeField] protected float attackCooldown = 2f;
  [HideIf("passiveMob")] [HideInInspector] public bool isAttackOnCooldown;

  // effects
  [SerializeField] private bool addHitEffect = false;
  [ShowIf("addHitEffect")] [SerializeField] private GameObject hitSmoke;

  // animations
  protected const string IDLE_ANIMATION = "EnemyIdle";
  protected const string WALK_ANIMATION = "EnemyWalk";
  protected const string DEATH_ANIMATION = "EnemyDeath";
  protected const string REAPPEAR_ANIMATION = "EnemyReappear";
  protected const string ATTACK_PREPARATION_ANIMATION = "EnemyAttackPreparation";
  protected const string ATTACK_ANIMATION = "EnemyAttack";
  [HideInInspector] public bool isAttacking = false;
  protected float deathAnimDuration;
  [HideInInspector] public bool isCreatureDead = false;
  protected virtual void Awake()
  {
    _playerStateController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();
    _playerCombat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombatController>();
    _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

    sprRenderer = GetComponent<SpriteRenderer>();
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();

    bodyHitboxCollider = transform.GetChild(0).GetComponent<Collider2D>();

    UpdateAnimClipTimes();
  }

  protected virtual void Update()
  {
    InvertSprite();
    HandleAnimation();
  }

  public virtual void HandleAttack()
  {
    if (isAttacking || isAttackOnCooldown || isCreatureDead || passiveMob)
      return;

    isAttacking = true;
    anim.Play(ATTACK_PREPARATION_ANIMATION);
    Invoke(nameof(Attack), attackPreparationTime);
    Invoke(nameof(AttackEnd), attackDuration + attackPreparationTime);
    StartCoroutine(setAttackCooldown());
  }

  protected IEnumerator setAttackCooldown()
  {
    isAttackOnCooldown = true;
    yield return new WaitForSeconds(attackCooldown);
    isAttackOnCooldown = false;
  }

  protected virtual void Attack()
  {
    if (isCreatureDead)
      return;

    anim.Play(ATTACK_ANIMATION);
  }

  protected virtual void AttackEnd()
  {
    isAttacking = false;
    anim.Play(IDLE_ANIMATION);
  }

  protected void DamagePlayer()
  {
    _playerCombat.PlayerLosesHealth(1);
  }

  protected virtual void OnCreatureHit()
  {
    anim.Play(DEATH_ANIMATION);
    isCreatureDead = true;
    bodyHitboxCollider.enabled = false;
    Invoke(nameof(DeactivateSpriteRenderer), deathAnimDuration);

    if (reappear)
      Invoke(nameof(Reappear), delayToReappear);
  }

  protected virtual void Reappear()
  {
    anim.Play(REAPPEAR_ANIMATION);
    sprRenderer.enabled = true;
    isCreatureDead = false;
    bodyHitboxCollider.enabled = true;
  }

  protected virtual void OnTriggerEnter2D(Collider2D col)
  {
    bool playerIsAttacking = _playerStateController.GetCurrentState() == PlayerState.Dashing;

    if (playerIsAttacking && col.gameObject.tag == "PlayerAttackHitbox")
    {
      OnCreatureHit();

      if (addHitEffect)
        SendDamageEffect();

      return;
    }

    if (!playerIsAttacking && col.gameObject.tag == "Player" && !passiveMob)
      DamagePlayer();
  }

  protected virtual void OnTriggerStay2D(Collider2D col) 
  {
    bool playerIsAttacking = _playerStateController.GetCurrentState() == PlayerState.Dashing;

    if (playerIsAttacking && col.gameObject.tag == "PlayerAttackHitbox")
      OnCreatureHit();
  }

  protected virtual void InvertSprite()
  {
    if (math.abs(rb.velocity.x) < 0.1f)
      return;

    sprRenderer.flipX = rb.velocity.x > 0.1f ? true : false;
  }

  protected virtual void HandleAnimation()
  {
    if (isCreatureDead || isAttacking)
      return;

    if (math.abs(rb.velocity.x) > 0.1f)
    {
      anim.Play(WALK_ANIMATION);
      return;
    }

    anim.Play(IDLE_ANIMATION);
  }

  protected void UpdateAnimClipTimes()
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

  protected void DeactivateSpriteRenderer()
  {
    sprRenderer.enabled = false;
  }

  protected void SendDamageEffect()
  {
    Vector2 effectPosition = GetPositionRelativeToPlayer();
    Instantiate(hitSmoke, effectPosition, Quaternion.identity, transform);
  }

  protected Vector2 GetPositionRelativeToPlayer()
  {
    Vector2 middlePos = bodyHitboxCollider.bounds.center;
    Vector2 topPos = new Vector2(bodyHitboxCollider.bounds.center.x, bodyHitboxCollider.bounds.center.y + transform.localScale.y / 1.5f);
    Vector2 bottomPos = new Vector2(bodyHitboxCollider.bounds.center.x, bodyHitboxCollider.bounds.center.y - transform.localScale.y / 1.5f);
    Vector2 leftPos = middlePos + Vector2.left;
    Vector2 rightPos = middlePos + Vector2.right;

    Vector2 playerPos = _playerStateController.transform.position;
    float smallestDistance = Vector2.Distance(topPos, playerPos);
    Vector2 idealPosition = new Vector2(playerPos.x, topPos.y);

    if (Vector2.Distance(bottomPos, playerPos) < smallestDistance)
    {
      smallestDistance = Vector2.Distance(bottomPos, playerPos);
      idealPosition = new Vector2(playerPos.x, bottomPos.y);
    }

    if (Vector2.Distance(leftPos, playerPos) < smallestDistance)
    {
      smallestDistance = Vector2.Distance(leftPos, playerPos);
      idealPosition = new Vector2(leftPos.x, playerPos.y);
    }

    if (Vector2.Distance(rightPos, playerPos) < smallestDistance)
      idealPosition = new Vector2(rightPos.x, playerPos.y);

    return idealPosition;
  }
}
