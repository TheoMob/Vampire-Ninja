using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
  [SerializeField] bool deactivated = false;
  [SerializeField] private Vector2[] patrolPointGizmos;
  private Vector2[] patrolPoints;
  private int currentPatrolIndex = 0;
  [SerializeField] private float speed = 100f;
  [SerializeField] private float delayToStartWorking = 2;
  [SerializeField] private float limitDistanceToEdge = 0.1f;

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
    StartCoroutine(InitialDelay());

    patrolPoints = patrolPointGizmos; // patrolPointGizmos have local position, while patrolPoints have global positions
    for(int i = 0; i < patrolPointGizmos.Length; i++)
      patrolPoints[i] = new Vector2(patrolPointGizmos[i].x + transform.position.x, patrolPointGizmos[i].y + transform.position.y);
  }

  private void FixedUpdate()
  {
    if (deactivated)
      return;

    PlatformMovement();
  }

  private void PlatformMovement()
  {
    int nextPatrolIndex = currentPatrolIndex + 1;

    if (nextPatrolIndex >= patrolPoints.Length || nextPatrolIndex < 0) // Reached the end of the path
        return;

    float distanceToEdgePoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex]);
    if (distanceToEdgePoint < limitDistanceToEdge) // reached one of the points
    {
      currentPatrolIndex = nextPatrolIndex;
      return;
    }

    transform.position = Vector3.MoveTowards(transform.position, patrolPoints[currentPatrolIndex], speed * Time.deltaTime);
  }

  private IEnumerator InitialDelay()
  {
    deactivated = true;
    yield return new WaitForSeconds(delayToStartWorking);
    deactivated = false;
  }
}
