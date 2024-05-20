using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
  [SerializeField] bool deactivated = false;
  [SerializeField] float delayToStartWorking = 0f; // this is so platforms can have different timings from one another
  [SerializeField] private Vector2[] patrolPointGizmos;
  private Vector2[] patrolPoints;
  private int currentPatrolIndex = 0;
  [SerializeField] private float speed = 100f;
  [SerializeField] private float delayMoveToNextPoint = 0.2f;
  [SerializeField] private float limitDistanceToEdge = 0.1f;
  [SerializeField] private bool useDeceleration = false;
  [SerializeField] private bool ciclicPathing = false;
  private BoxCollider2D platformCollider;
  private bool isInCooldown = false;
  private bool returningPath = false;

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
    StartCoroutine(InitialDelay()); // this is so platforms can have different timings from one another

    platformCollider = GetComponent<BoxCollider2D>();
    patrolPoints = patrolPointGizmos; // patrolPointGizmos have local position, while patrolPoints have global positions
    for(int i = 0; i < patrolPointGizmos.Length; i++)
      patrolPoints[i] = new Vector2(patrolPointGizmos[i].x + transform.position.x, patrolPointGizmos[i].y + transform.position.y);
  }

  private void FixedUpdate()
  {
    if (deactivated || isInCooldown)
      return;

    PlatformMovement();
  }

  private void PlatformMovement()
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

    float distanceToEdgePoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex]);
    if (distanceToEdgePoint < limitDistanceToEdge) // this means the trap has reached one of the patrolPoints
    {
      currentPatrolIndex = nextPatrolIndex;
      StartCoroutine(MovementCooldown(delayMoveToNextPoint));
      return;
    }

    float currentSpeed = speed;
    if (useDeceleration)
    {
      float maxAcceleration = 0.1f;
      float maxDecceleration = 4f;
      float deceleration = 1f - (distanceToEdgePoint / Vector2.Distance(patrolPoints[currentPatrolIndex], patrolPoints[nextPatrolIndex]));
      currentSpeed = Mathf.Lerp(speed * maxDecceleration, speed * maxAcceleration, deceleration);
    }

    transform.position = Vector3.MoveTowards(transform.position, patrolPoints[currentPatrolIndex], currentSpeed * Time.deltaTime);
  }

  private IEnumerator MovementCooldown(float cooldown)
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

  private void OnCollisionEnter2D(Collision2D other) // this makes so the player becomes attached to the platform, that way he follow the movement that the platform does
  {
    if (!other.gameObject.CompareTag("Player"))
      return;

    bool isPlayerSteppingOnPlatform = Physics2D.BoxCast(platformCollider.bounds.center, platformCollider.size, 0, Vector2.up, 0.1f, LayerMask.GetMask("Player")); // test the collision on the upper side of the platform

    if (isPlayerSteppingOnPlatform)
      other.gameObject.transform.SetParent(gameObject.transform);
  }

  private void OnCollisionExit2D(Collision2D other) // deattach the player from the platform when he's no longer colliding with it
  {
    if (!other.gameObject.CompareTag("Player"))
      return;

    other.gameObject.transform.SetParent(null);
  }
}
