using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WalkCreature : BaseCreature
{
    [Header("Walk Variables")]
    [SerializeField] protected WalkType walkType;
    [ShowIf("walkType", WalkType.betweenTwoPoints)] [SerializeField] protected Vector2[] walkPoints = new Vector2[2];
    [ShowIf("walkType", WalkType.betweenTwoPoints)] [SerializeField] protected float timeStoppedInEachPoint;
    [SerializeField] protected float walkSpeed;

    protected int walkPointIndex;
    protected float limitDistanceToEdge = 0.1f;
    protected bool isIdle;

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (walkPoints != null)
        {
            foreach (Vector2 walkPoint in walkPoints)
            {
                if (walkPoint != null)
                {   
                    Vector2 drawPosition = new Vector2(walkPoint.x + transform.position.x, walkPoint.y + transform.position.y);
                    if (Application.isPlaying)
                        drawPosition = walkPoint;
                    
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

        walkPointIndex = 0;
        for (int i = 0; i < walkPoints.Length; i++)
            walkPoints[i] = new Vector2(walkPoints[i].x + transform.position.x, walkPoints[i].y + transform.position.y);
    }

    protected virtual void FixedUpdate() 
    {
        if (isDead)
            return;

        Walk();
        InvertSprite();
    }

    protected override void InvertSprite()
    {
        sprRenderer.flipX = walkPointIndex == 0 ? false : true;
    }

    protected virtual void Walk()
    {
        if (isIdle)
            return;

        float distanceToEdgePoint = Vector2.Distance(transform.position, walkPoints[walkPointIndex]);
        if (distanceToEdgePoint < limitDistanceToEdge) // this means the trap has reached one of the patrolPoints
        {
            StartCoroutine(IdleTimeHandler());
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, walkPoints[walkPointIndex], walkSpeed * Time.deltaTime);
    }

    protected IEnumerator IdleTimeHandler()
    {
        isIdle = true;
        yield return new WaitForSeconds(timeStoppedInEachPoint);
        isIdle = false;
        walkPointIndex = walkPointIndex == 0 ? 1 : 0;
    }

    public enum WalkType
    {
        betweenTwoPoints,
        untilBlocked,
    }
}
