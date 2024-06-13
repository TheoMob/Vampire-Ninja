using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using NaughtyAttributes;
using UnityEngine;

public class Traps : MonoBehaviour
{
  [SerializeField] private TrapType trapType;
  private GameObject trapObject; // body of the object
  private bool isInCooldown = false;

  [SerializeField] bool deactivated = false;
  [SerializeField] float delayToStartWorking = 0f; // this is so traps can have different timings from one another


  // Movement Hazard
  [Header("MovementHazard")]
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private Vector2[] patrolPointGizmos;
  private Vector2[] patrolPoints;
  private int currentPatrolIndex = 0;
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private float speed = 100f;
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private float delayMoveToNextPoint = 0.2f;
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private float limitDistanceToEdge = 0.1f;
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private bool useDeceleration = false;
  [ShowIf("trapType", TrapType.MovementHazard)] [SerializeField] private bool ciclicPathing = false;
  [ShowIf("trapType", TrapType.MovementHazard)] private bool returningPath = false;

  // Trap that shoots arrows
  [Header("ProjectileShooter")]
  private GameObject projectile;
  [ShowIf("trapType", TrapType.ProjectileShooter)] [SerializeField] private ShootDirection shootDirection;
  [ShowIf("trapType", TrapType.ProjectileShooter)] [SerializeField] private float shootForce;
  [ShowIf("trapType", TrapType.ProjectileShooter)] [SerializeField] private float delayToShoot;
  [ShowIf("trapType", TrapType.ProjectileShooter)] [SerializeField] private float shootCooldown;
  private Animator trapAnimator;
  private const string SHOOT_ANIMATION = "ArrowShoot";
  private const string SHOOT_READY = "ArrowReady";

  #if UNITY_EDITOR // this is so patrolPoints are drawn on the editor mode without having to create an object for them or run the game for them to appear
  private void OnDrawGizmos()
  {
    if (patrolPointGizmos != null)
    {
      foreach (Vector2 patrolPoint in patrolPointGizmos)
      {
        if (patrolPoint != null)
        {
          Gizmos.color = Color.green;
          Gizmos.DrawSphere(new Vector2(patrolPoint.x + transform.position.x, patrolPoint.y + transform.position.y), .3f);
        }
      }
    }
  }
  #endif

  private void Awake()
  {
    StartCoroutine(InitialDelay()); // this is so traps can have different timings from one another

    switch(trapType)
    {
      case TrapType.MovementHazard:
        patrolPoints = patrolPointGizmos; // patrolPointGizmos have local position, while patrolPoints have global positions
        for(int i = 0; i < patrolPointGizmos.Length; i++)
          patrolPoints[i] = new Vector2(patrolPointGizmos[i].x + transform.position.x, patrolPointGizmos[i].y + transform.position.y);

        trapObject = transform.GetChild(0).gameObject;
      break;

      case TrapType.ProjectileShooter:
        projectile = transform.GetChild(1).gameObject;
        projectile.SetActive(false);

        trapObject = transform.GetChild(0).gameObject;

        trapAnimator = trapObject.GetComponent<Animator>();
      break;
    }
  }
  private float maxAcceleration = 0.1f;
  private float maxDecceleration = 4f;
  private void FixedUpdate()
  {
    if (deactivated || isInCooldown)
      return;

    switch(trapType)
    {
      case TrapType.MovementHazard:
        MovementHazard();
      break;
      case TrapType.ProjectileShooter:
        isInCooldown = true;

        trapAnimator.Play(SHOOT_READY);

        Invoke("ProjectileShooter", delayToShoot);
      break;
    }
  }

  #region MovementHazard
  private void MovementHazard()
  {
    int nextPatrolIndex = returningPath ? currentPatrolIndex - 1 : currentPatrolIndex + 1;

    if (nextPatrolIndex >= patrolPoints.Length || nextPatrolIndex < 0) // invert the path if reached it's end
    {
      if (!ciclicPathing) // if not ciclic, do the path but backwards, otherwise go directly to the initial position
      {
        returningPath = !returningPath;
        nextPatrolIndex = returningPath ? currentPatrolIndex - 1 : currentPatrolIndex + 1;
      }
      else
        nextPatrolIndex = 0;
    }

    float distanceToEdgePoint = Vector3.Distance(trapObject.transform.position, patrolPoints[currentPatrolIndex]);
    if (distanceToEdgePoint < limitDistanceToEdge) // this means the trap has reached one of the patrolPoints
    {
      currentPatrolIndex = nextPatrolIndex;
      StartCoroutine(TrapCooldown(delayMoveToNextPoint));
      return;
    }

    float currentSpeed = speed;
    if (useDeceleration)
    {
      float deceleration = 1f - (distanceToEdgePoint / Vector2.Distance(patrolPoints[currentPatrolIndex], patrolPoints[nextPatrolIndex]));
      currentSpeed = Mathf.Lerp(speed * maxDecceleration, speed * maxAcceleration, deceleration);
    }

    trapObject.transform.position = Vector3.MoveTowards(trapObject.transform.position, patrolPoints[currentPatrolIndex], currentSpeed * Time.deltaTime);
  }
  #endregion

  #region ProjectileShooter
  private void ProjectileShooter()
  {
    Vector2 direction = getShootDirection();
    Vector2 arrowOffset = -0.1f * direction; // this is so the arrow dont spawn exactly in the middle, but rather on the exit of the trap, this is related to the sprite, so it needs to change in case the sprite does
    Vector2 spawnPosition = arrowOffset + (Vector2)transform.position;

    GameObject arrowPrefab = Instantiate(projectile, spawnPosition, transform.rotation, gameObject.transform);
    arrowPrefab.SetActive(true);

    Rigidbody2D rb = arrowPrefab.GetComponent<Rigidbody2D>();
    rb.AddForce(direction * shootForce, ForceMode2D.Impulse);

    trapAnimator.Play(SHOOT_ANIMATION);

    StartCoroutine(TrapCooldown(shootCooldown));
  }
  #endregion

  private Vector2 getShootDirection()
  {
    if (shootDirection == ShootDirection.Up)
      return Vector2.up;
    if (shootDirection == ShootDirection.Down)
      return Vector2.down;
    if (shootDirection == ShootDirection.Left)
      return Vector2.left;

    return Vector2.right;
  }

  private IEnumerator TrapCooldown(float cooldown)
  {
    isInCooldown = true;
    yield return new WaitForSeconds(cooldown);
    isInCooldown = false;
  }

  private IEnumerator InitialDelay()
  {
    deactivated = true;
    yield return new WaitForSeconds(delayToStartWorking);
    deactivated = false;
  }

  public enum TrapType
  {
    MovementHazard,
    ProjectileShooter,
    LandMine,
  }

  private enum ShootDirection
  {
    Up,
    Down,
    Left,
    Right
  }
}
