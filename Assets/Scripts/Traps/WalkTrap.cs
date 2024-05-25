using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkTrap : Trap
{
    [SerializeField] private Vector2[] patrolPoints;
    private int currentPatrolIndex = 0;
    [SerializeField] private float speed = 100f;
    [SerializeField] private bool useDeceleration = false;
    [SerializeField] private bool ciclicPathing = false;
    private float limitDistanceToEdge = 0.1f;
    private bool returningPath = false;

    #if UNITY_EDITOR // this is so patrolPoints are drawn on the editor mode without having to create an object for them or run the game for them to appear
    private void OnDrawGizmos()
    {
        if (patrolPoints != null)
        {
            foreach (Vector2 patrolPoint in patrolPoints)
            {
                if (patrolPoint != null)
                {   
                    Vector2 drawPosition = new Vector2(patrolPoint.x + transform.position.x, patrolPoint.y + transform.position.y);
                    if (Application.isPlaying)
                        drawPosition = patrolPoint;
                    
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(drawPosition, .3f);
                }
            }
        }
    }
    #endif

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < patrolPoints.Length; i++)
            patrolPoints[i] = new Vector2(patrolPoints[i].x + transform.position.x, patrolPoints[i].y + transform.position.y);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void TrapFunction()
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
            StartCoroutine(TrapCooldown());
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
}
