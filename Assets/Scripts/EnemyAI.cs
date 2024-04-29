using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Pathfinding;
public class EnemyAI : MonoBehaviour
{
  private EnemyCreature _enemyCreature;
  private Seeker seeker;
  private Rigidbody2D rb;
  private Transform target;
  private BoxCollider2D col;

  [SerializeField] private float speed = 200f;
  [SerializeField] private float nextWaypointDistance = 3f;
  [SerializeField] private float startSeekingDistance = 10f;
  [SerializeField] private float stopSeekingDistance = 15f;
  [SerializeField] private float attackingDistance = 5f;
  [HideInInspector] public bool isCreatureDead = false;

  Path path;
  private int currentWaypoint = 0;
  private bool reachedEndOfPath = false;

  private Vector2 initialPosition;
  private AIState aIState;
  void Awake()
  {
    _enemyCreature = GetComponent<EnemyCreature>();
    seeker = GetComponent<Seeker>();
    rb = GetComponent<Rigidbody2D>();
    col = GetComponent<BoxCollider2D>();
    target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

    initialPosition = rb.position;
  }

  private bool switchState = false;
  void FixedUpdate()
  {
    if (isCreatureDead)
      return;

    float targetDistance = Vector2.Distance(target.position, rb.position);

    switch(aIState)
    {
      case AIState.Patroling:
        if (switchState)
        {
          InvokeRepeating("UpdateSeekingPath", 0f, .5f);
          switchState = false;
        }

        if (targetDistance < startSeekingDistance)
        {
          aIState = AIState.SeekingTarget;
          switchState = true;
          CancelInvoke(nameof(UpdateSeekingPath));
          break;
        }

        if (jumpEnabled)
          PathFollow();
        else
          seekTarget();
      break;
      case AIState.SeekingTarget:
        if (switchState)
        {
          InvokeRepeating("UpdateSeekingPath", 0f, .5f);
          switchState = false;
        }

        if (targetDistance > stopSeekingDistance)
        {
          aIState = AIState.Patroling;
          switchState = true;
          CancelInvoke(nameof(UpdateSeekingPath));
          break;
        }

        checkAttackRange();

        if (targetDistance < attackingDistance && checkAttackRange())
        {
          aIState = AIState.Attacking;
          switchState = true;
          CancelInvoke(nameof(UpdateSeekingPath));
          break;
        }

        if (jumpEnabled)
          PathFollow();
        else
          seekTarget();
      break;
      case AIState.Attacking:
        if (_enemyCreature.isAttacking)
          break;

        if (targetDistance > attackingDistance)
        {
          aIState = AIState.Patroling;
          switchState = true;
          break;
        }

      _enemyCreature.HandleAttack();
      break;
    }
  }

  private bool checkAttackRange()
  {
    bool attackAligned = Physics2D.BoxCast(col.bounds.center, col.size, 0, Vector2.left * Math.Sign(transform.localScale.x), attackingDistance, LayerMask.GetMask("Player"));

    #if UNITY_EDITOR
    Vector2 lookingDirection = Vector2.left * Math.Sign(transform.localScale.x) * attackingDistance;
    Color drawColor = attackAligned ? Color.green : Color.red;
    Vector2 startPoint = col.bounds.center;
    Vector2 endPoint = new Vector2(startPoint.x + lookingDirection.x, startPoint.y + lookingDirection.y);
    Debug.DrawLine(startPoint, endPoint, drawColor);
    #endif

    return attackAligned;
  }

  private void UpdateSeekingPath()
  {
    Vector2 targetPosition = aIState == AIState.SeekingTarget ? target.position : initialPosition;

    if (seeker.IsDone())
      seeker.StartPath(rb.position, targetPosition, OnPathComplete);
  }

  private void OnPathComplete(Path p)
  {
    if (p.error)
      return;

    path = p;
    currentWaypoint = 0;
  }

  private void seekTarget()
  {
    if (path == null)
      return;

    if (currentWaypoint >= path.vectorPath.Count)
    {
      reachedEndOfPath = true;
      return;
    }
    else
    {
      reachedEndOfPath = false;
    }

    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
    Vector2 force = direction * speed * Time.deltaTime;

    rb.AddForce(force);

    float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

    if (distance < nextWaypointDistance)
    {
      currentWaypoint++;
    }
  }

  #region seekerJump

  [Header("Jumping")]
  public bool jumpEnabled = true, isJumping, isInAir;
  private bool isGrounded;
  [SerializeField] private float jumpNodeHeightRequirement = 0.8f;
  [SerializeField] private float jumpCheckOffset = 0.1f;
  [SerializeField] private float jumpForce = 100f;
  [SerializeField] Vector3 startOffset;
  private bool isOnCooldown;
  private void initiateJumpingVariables()
  {
    isOnCooldown = false;
    isInAir = false;
    isJumping = false;
  }
  private void PathFollow()
  {
    if (path == null)
      return;

    // Reached end of path
    if (currentWaypoint >= path.vectorPath.Count)
      return;

    // See if colliding with anything
    startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset, transform.position.z);

    isGrounded = Physics2D.BoxCast(col.bounds.center, col.size, 0, Vector2.down, 0.05f, LayerMask.GetMask("Ground"));

    // Direction Calculation
    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
    Vector2 force = direction * speed;

    // Jump
    if (jumpEnabled && isGrounded && !isInAir && !isOnCooldown)
    {
        if (direction.y > jumpNodeHeightRequirement)
        {
          if (isInAir) return;
          isJumping = true;
          rb.velocity = new Vector2(rb.velocity.x, jumpForce);
          StartCoroutine(JumpCoolDown());
        }
    }
    if (isGrounded)
    {
      isJumping = false;
      isInAir = false;
    }
    else
    {
      isInAir = true;
    }

    rb.velocity = new Vector2(force.x, rb.velocity.y);

    float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
    if (distance < nextWaypointDistance)
    {
      currentWaypoint++;
    }
  }

  IEnumerator JumpCoolDown()
  {
    isOnCooldown = true;
    yield return new WaitForSeconds(1f);
    isOnCooldown = false;
  }
  #endregion

  public enum AIState
  {
    Patroling,
    SeekingTarget,
    Attacking
  }
}
