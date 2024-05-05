using System;
using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEditor;
using Unity.VisualScripting;
public class EnemyAI : MonoBehaviour
{
  private EnemyCreature _enemyCreature;
  private Seeker seeker;
  private Rigidbody2D rb;
  private Transform target;
  private BoxCollider2D col;

  private GameObject body;

  [SerializeField] private float speed = 200f;
  [SerializeField] private float nextWaypointDistance = 1f;
  [SerializeField] private float startSeekingDistance = 10f;
  [SerializeField] private float stopSeekingDistance = 15f;
  [SerializeField] private float attackingDistance = 5f;


  [Header("Patrol")]
  [SerializeField] private bool dontPatrol = false;
  [SerializeField] private Vector2[] patrolPointGizmos; // used to draw the gizmos on the screen of the patrolPoints
  private Vector2[] patrolPoints; // global position of the patrolPointGizmos, used to actually mark the patrolPoints and not to draw them
  [SerializeField] private float timeIdleInEachPatrolPoint = 1f;
  private bool isPatrolIdle = false; // variable that makes sure that the timeIdleInEachPatrolPoint is respected

  [HideInInspector] public bool isCreatureDead = false;
  Path path;
  private int currentWaypoint = 0;
  private bool reachedEndOfPath = false;

  private Vector2 initialPosition;
  private AIState aIState;

  #if UNITY_EDITOR // this is so patrolPoints are drawn on the editor mode without having to create an object for them or run the game for them to appear
    private void OnDrawGizmos()
    {
      if (patrolPointGizmos == null)
        return;

      foreach (Vector2 patrolPoint in patrolPointGizmos)
      {
        if (patrolPoint != null)
        {
          Gizmos.color = Color.green;
          Gizmos.DrawSphere(new Vector2(patrolPoint.x + transform.position.x, patrolPoint.y + transform.position.y), .3f);
        }
      }
    }
  #endif
  void Awake()
  {
    _enemyCreature = GetComponent<EnemyCreature>();
    target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

    foreach(Transform child in transform)
    {
      if (child.CompareTag("Body"))
      {
        body = child.gameObject;
        seeker = child.GetComponent<Seeker>();
        rb = child.GetComponent<Rigidbody2D>();
        col = child.GetComponent<BoxCollider2D>();
        break;
      }
    }

    initialPosition = rb.position;
    InvokeRepeating("UpdateSeekingPath", 0f, 1f);

    patrolPoints = patrolPointGizmos; // patrolPointGizmos have local position, while patrolPoints have global positions
    for(int i = 0; i < patrolPointGizmos.Length; i++)
    {
      patrolPoints[i] = new Vector2(patrolPointGizmos[i].x + transform.position.x, patrolPointGizmos[i].y + transform.position.y);
    }
  }

  private void UpdateSeekingPath()
  {
    Vector2 targetPosition = initialPosition;

    if (aIState == AIState.SeekingTarget)
      targetPosition = target.position;

    if (seeker.IsDone())
      seeker.StartPath(rb.position, targetPosition, OnPathComplete);
  }

  void FixedUpdate()
  {
    //Debug.Log(aIState);
    if (isCreatureDead)
      return;

    float targetDistance = Vector2.Distance(target.position, rb.position);

    switch(aIState)
    {
      case AIState.Idle:
        if (targetDistance < startSeekingDistance)
        {
          aIState = AIState.SeekingTarget;
          break;
        }

        if (Vector2.Distance(rb.position, initialPosition) > nextWaypointDistance)
          SeekTarget();
      break;

      case AIState.Patroling:
        if (dontPatrol)
        {
          aIState = AIState.Idle;
          break;
        }

        if (targetDistance < startSeekingDistance)
        {
          aIState = AIState.SeekingTarget;
          break;
        }

        if (!isPatrolIdle)
          SeekTarget();
      break;

      case AIState.SeekingTarget:
        if (targetDistance > stopSeekingDistance)
        {
          aIState = AIState.Patroling;
          break;
        }

        checkAttackRange();

        if (targetDistance < attackingDistance && checkAttackRange() && !_enemyCreature.isAttackOnCooldown)
        {
          aIState = AIState.Attacking;
          break;
        }

        SeekTarget();
      break;

      case AIState.Attacking:
        if (_enemyCreature.isAttacking)
          break;

        if (targetDistance > attackingDistance)
        {
          aIState = AIState.Patroling;
          break;
        }

      _enemyCreature.HandleAttack();
      break;
    }
  }

  private bool checkAttackRange()
  {
    bool attackAligned = Physics2D.BoxCast(col.bounds.center, col.size, 0, Vector2.left * Math.Sign(body.transform.localScale.x), attackingDistance, LayerMask.GetMask("Player"));

    #if UNITY_EDITOR
    Vector2 lookingDirection = Vector2.left * Math.Sign(body.transform.localScale.x) * attackingDistance;
    Color drawColor = attackAligned ? Color.green : Color.red;
    Vector2 startPoint = col.bounds.center;
    Vector2 endPoint = new Vector2(startPoint.x + lookingDirection.x, startPoint.y + lookingDirection.y);
    Debug.DrawLine(startPoint, endPoint, drawColor);
    #endif

    return attackAligned;
  }

  private void OnPathComplete(Path p)
  {
    if (p.error)
      return;

    path = p;

    bool resetWayPoint = aIState != AIState.Patroling || (aIState == AIState.Patroling && currentWaypoint >= patrolPoints.Length);

    if (resetWayPoint)
      currentWaypoint = 0;
  }

  #region seekTarget

  [Header("Types of seeking paths")]
  [SerializeField] private SeekType seekType;

  [Header("Jump parameters")]
  [SerializeField] private float jumpNodeHeightRequirement = 0.8f;
  [SerializeField] private float jumpForce = 100f;
  [SerializeField] private float jumpCooldown = 1f;
  private bool isJumpOnCooldown = false;
  private bool isGrounded;

  private void SeekTarget()
  {
    Debug.Log(aIState);

    if (path == null)
      return;

    reachedEndOfPath = currentWaypoint >= path.vectorPath.Count;

    if (reachedEndOfPath)
      return;

    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
    float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

    if (aIState == AIState.Patroling) // if the character is patrolling, then his path is determined by the patrolPoints, the other cases the path is determined by the targetPosition
    {
      direction = ((Vector2)patrolPoints[currentWaypoint % patrolPoints.Length] - rb.position).normalized;
      distance = Vector2.Distance(rb.position, (Vector2)patrolPoints[currentWaypoint % patrolPoints.Length]);
    }

    Vector2 force = direction * speed * Time.deltaTime;
    switch(seekType)
    {
      case SeekType.Flying:
        rb.AddForce(force);
      break;

      case SeekType.Walking:
        isGrounded = Physics2D.BoxCast(col.bounds.center, col.size, 0, Vector2.down, 0.05f, LayerMask.GetMask("Ground"));

        if (isGrounded && !isJumpOnCooldown)
        {
          if (direction.y > jumpNodeHeightRequirement)
          {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            StartCoroutine(JumpCoolDown());
          }
        }

        rb.velocity = new Vector2(force.x, rb.velocity.y);
      break;
    }

    if (distance < nextWaypointDistance)
    {
      currentWaypoint++;
      if (aIState == AIState.Patroling)
      {
        StartCoroutine(PatrolIdleTime());
      }
    }
  }

  private IEnumerator PatrolIdleTime()
  {
    isPatrolIdle = true;
    yield return new WaitForSeconds(timeIdleInEachPatrolPoint);
    isPatrolIdle = false;
  }

  IEnumerator JumpCoolDown()
  {
    isJumpOnCooldown = true;
    yield return new WaitForSeconds(jumpCooldown);
    isJumpOnCooldown = false;
  }

  #endregion

  public enum AIState
  {
    Idle,
    Patroling,
    SeekingTarget,
    Attacking
  }

  public enum SeekType
  {
    Flying,
    Walking,
  }
}