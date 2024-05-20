using System;
using System.Collections;
using UnityEngine;
using Pathfinding;
using NaughtyAttributes;
public class EnemyAI : MonoBehaviour
{
  private EnemyCreature _enemyCreature;
  private Seeker seeker;
  private Rigidbody2D rb;
  private Transform target;
  private BoxCollider2D col;
  private SpriteRenderer sprRenderer;

  [SerializeField] private float speed = 200f;
  private float nextWaypointDistance = 1f;
  [SerializeField] private float startSeekingDistance = 10f;
  [SerializeField] private float stopSeekingDistance = 15f;
  [SerializeField] private float attackingDistance = 5f;
  [SerializeField] private float attackRange = 5f;

  // Patrolling variables
  [SerializeField] private bool patrolBetweenPositions;
  [Header("Patrol")]
  [ShowIf("patrolBetweenPositions")] [SerializeField] private Vector2[] patrolPointGizmos; // used to draw the gizmos on the screen of the patrolPoints
  [ShowIf("patrolBetweenPositions")] [SerializeField] private float timeIdleInEachPatrolPoint = 1f;
  private Vector2[] patrolPoints; // global position of the patrolPointGizmos, used to actually mark the patrolPoints and not to draw them
  private bool isPatrolIdle = false; // variable that makes sure that the timeIdleInEachPatrolPoint is respected

  //A* pathfinding variables
  Path path;
  private int currentWaypoint = 0;
  private Vector2 initialPosition;
  public AIState aIState;

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
    target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

    foreach(Transform child in transform)
    {
      if (child.CompareTag("Body"))
      {
        seeker = child.GetComponent<Seeker>();
        rb = child.GetComponent<Rigidbody2D>();
        col = child.GetComponent<BoxCollider2D>();
        sprRenderer = child.GetComponent<SpriteRenderer>();
        _enemyCreature = child.GetComponent<EnemyCreature>();
        break;
      }
    }

    initialPosition = rb.position;
    InvokeRepeating("UpdateSeekingPath", 0f, .5f);

    patrolPoints = patrolPointGizmos; // patrolPointGizmos have local position, while patrolPoints have global positions
    for(int i = 0; i < patrolPointGizmos.Length; i++)
    {
      patrolPoints[i] = new Vector2(patrolPointGizmos[i].x + transform.position.x, patrolPointGizmos[i].y + transform.position.y);
    }
  }
  private void UpdateSeekingPath()
  {
    Vector2 endPosition = target.position;

    if (aIState == AIState.Patroling)
    {
      if (patrolPoints.Length > 0)
        endPosition = patrolPoints[currentWaypoint % patrolPoints.Length];
      else
        endPosition = initialPosition;
    }

    if (seeker.IsDone())
      seeker.StartPath(rb.position, endPosition, OnPathComplete);
  }

  void FixedUpdate()
  {
    if (_enemyCreature.isCreatureDead)
      return;

    AIStateMachine();
  }

  #region AIState Machine
  private void AIStateMachine()
  {
    aIState = getAIState();

    checkAttackRange(); // this is for debug purposes only
    //Debug.Log(aIState);

    switch(aIState)
    {
      case AIState.Patroling:
        if (!isPatrolIdle)
          Patrol();
      break;

      case AIState.SeekingTarget:
        SeekTarget();
      break;

      case AIState.Attacking:
        if (_enemyCreature.isAttacking)
          break;

        if (checkAttackRange() && !_enemyCreature.isAttackOnCooldown)
          _enemyCreature.HandleAttack();
        else
          SeekTarget();
      break;
    }
  }

  private bool forceAttackState = false;
  private AIState getAIState()
  {
    float targetDistance = Vector2.Distance(rb.position, target.position);

    if (forceAttackState)
      return AIState.Attacking;

    if (targetDistance > stopSeekingDistance)
      return AIState.Patroling;

    if (_enemyCreature.isAttacking || targetDistance < attackingDistance)
      return AIState.Attacking;

    if (targetDistance < startSeekingDistance)
      return AIState.SeekingTarget;

    if (aIState == AIState.SeekingTarget && targetDistance < stopSeekingDistance)
      return AIState.SeekingTarget;

    return AIState.Patroling;
  }

  private bool checkAttackRange()
  {
    Vector2 lookingDirection = sprRenderer.flipX ? Vector2.right : Vector2.left;
    Vector2 startRangePos = col.bounds.center;
    if (seekType == SeekType.KeepDistanceOnGround)
      startRangePos = startRangePos + (desiredDistanceFromTarget / 2 * lookingDirection);
    Vector2 endRangePos = startRangePos + (attackRange * lookingDirection);

    bool attackAligned = Physics2D.Linecast(startRangePos, endRangePos, LayerMask.GetMask("Player"));

    #if UNITY_EDITOR
    lookingDirection *= attackingDistance;
    Color drawColor = attackAligned ? Color.green : Color.red;
    Debug.DrawLine(startRangePos, endRangePos, drawColor);
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

  #endregion

  #region seekTarget

  [Header("Types of seeking paths")]
  [SerializeField] private SeekType seekType;

  [ShowIf("seekType", SeekType.ChaseOnGround)] [SerializeField] private float jumpNodeHeightRequirement = 0.8f;
  [ShowIf("seekType", SeekType.ChaseOnGround)] [SerializeField] private float jumpForce = 100f;
  [ShowIf("seekType", SeekType.ChaseOnGround)] [SerializeField] private float jumpCooldown = 1f;

  [ShowIf("seekType", SeekType.KeepDistanceOnGround)] [SerializeField] private float desiredDistanceFromTarget = 5f;
  [ShowIf("seekType", SeekType.KeepDistanceOnGround)] [SerializeField] private float distanceInterval = 2f; // it's like an offset for desiredDistanceFromTarget, so it doesn't have to be in that exact distance

  private float minimunDistanceToCountMovement = 0.3f;
  private bool isJumpOnCooldown = false;
  private bool isGrounded;

  private bool isPathValid(Path path) // tests if the path isn't null and if the path hasn't ended yet
  {
    if (path == null)
      return false;

    if (currentWaypoint >= path.vectorPath.Count)
      return false;

    return true;
  }
  private void SeekTarget() // makes the creature seek their target
  {
    if (!isPathValid(path))
      return;

    Vector2 nextPathPosition = path.vectorPath[currentWaypoint];
    Vector2 direction;
    Vector2 speedForce;
    Vector2 distance;

    switch(seekType)
    {
      case SeekType.ChaseOnAir:
        direction = (nextPathPosition - rb.position).normalized;
        speedForce = direction * speed * Time.deltaTime;

        rb.AddForce(speedForce);
      break;

      case SeekType.ChaseOnGround:
        isGrounded = Physics2D.BoxCast(col.bounds.center, col.size, 0, Vector2.down, 0.05f, LayerMask.GetMask("Ground"));

        distance = new Vector2(Math.Abs(target.position.x) - Math.Abs(rb.position.x), Math.Abs(target.position.y) - Math.Abs(rb.position.y));
        direction = GetDirection(rb.position, target.position);
        speedForce = direction  * speed * Time.deltaTime;

        if (isGrounded && !isJumpOnCooldown)
        {
          if (distance.y > jumpNodeHeightRequirement)
          {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            StartCoroutine(JumpCoolDown());
          }
        }

        speedForce.x = Math.Abs(distance.x) < minimunDistanceToCountMovement ? 0 : speedForce.x;
        rb.velocity = new Vector2(speedForce.x, rb.velocity.y);
      break;

      case SeekType.KeepDistanceOnGround:
        distance = new Vector2(Math.Abs(target.position.x - rb.position.x), Math.Abs(target.position.y - rb.position.y));

        direction = GetDirection(rb.position, target.position);
        speedForce = direction  * speed * Time.deltaTime;

        if (distance.x < desiredDistanceFromTarget) // walk backwards
          speedForce.x *= -1;

        if (Math.Abs(distance.x - desiredDistanceFromTarget) < distanceInterval) // if the creature is within the desiredDistanceInterval then dont move
          break;

        speedForce.x = distance.x < minimunDistanceToCountMovement ? 0 : speedForce.x;
        rb.velocity = new Vector2(speedForce.x, rb.velocity.y);
      break;
    }

    if (Vector2.Distance(rb.position, nextPathPosition) < nextWaypointDistance)
      currentWaypoint++;
  }

  #region Patrol
  private void Patrol() // makes the creature follow the patrolPoints, like he is scouting an area
  {
    if (!isPathValid(path))
      return;

    Vector2 nextPathPosition = initialPosition;

    if (patrolPoints.Length > 1) // if there is patrolPoints then he follows them, otherwise he just returns to it's original position
    {
      nextPathPosition = patrolPoints[currentWaypoint % patrolPoints.Length];
    }

    Vector2 direction = (nextPathPosition - rb.position).normalized;
    Vector2 speedForce = direction * speed * Time.deltaTime;

    switch(seekType)
    {
      case SeekType.ChaseOnAir:
        rb.AddForce(speedForce);
      break;

      case SeekType.ChaseOnGround:
        rb.velocity = new Vector2(speedForce.x, rb.velocity.y);
      break;

      case SeekType.KeepDistanceOnGround:
        rb.velocity = new Vector2(speedForce.x, rb.velocity.y);
      break;
    }

    if (Vector2.Distance(rb.position, nextPathPosition) < nextWaypointDistance)
    {
      currentWaypoint++;
      StartCoroutine(PatrolIdleTime());
    }
  }

  #endregion

  #region Enums and generic funcions
  private Vector2 GetDirection(Vector2 start, Vector2 end)
  {
    Vector2 direction = Vector2.zero;

    direction.x = end.x > start.x ? 1 : -1;
    direction.y = end.y > start.y ? 1 : -1;
    direction.x = end.x == start.x ? 0 : direction.x;
    direction.y = end.y == start.y ? 0 : direction.y;

    return direction;
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
    ChaseOnAir,
    ChaseOnGround,
    KeepDistanceOnGround,
  }

  #endregion
}