using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Traps : MonoBehaviour
{
  [SerializeField] private TrapType trapType;
  private GameObject trapObject;
  private bool isInCooldown = false;

  [Header("MovementHazard")]
  private Vector3[] pathEdges = new Vector3[2];
  [SerializeField] private float speed = 100f;
  [SerializeField] private float delayMoveToNextPoint = 0.2f;
  [SerializeField] private float limitDistanceToEdge = 0.1f;
  [SerializeField] private bool useDeceleration = false;
  [SerializeField] private bool returningPath = false;

  [Header("ProjectileShooter")]
  private GameObject projectile;
  [SerializeField] private Vector3 shootDirection;
  [SerializeField] private float shootForce;
  [SerializeField] private float shootCooldown;

  private void Awake()
  {
    switch(trapType)
    {
      case TrapType.MovementHazard:
        if (trapObject != null & pathEdges[0] != null && pathEdges[1] != null)
          return;

        trapObject = transform.GetChild(0).gameObject;
        pathEdges[0] = transform.GetChild(1).position;
        pathEdges[1] = transform.GetChild(2).position;
      break;

      case TrapType.ProjectileShooter:
        projectile = transform.GetChild(0).gameObject;
        projectile.SetActive(false);
      break;
    }
  }

  private float maxAcceleration = 0.1f;
  private float maxDecceleration = 4f;

  private void Update()
  {
    switch(trapType)
    {
      case TrapType.MovementHazard:
        if (isInCooldown)
          return;

        MovementHazard();
      break;
      case TrapType.ProjectileShooter:
        if (isInCooldown)
          return;

        ProjectileShooter();
      break;
    }
  }

  #region MovementHazard
  private void MovementHazard()
  {
    Vector2 pathEdge = returningPath ? pathEdges[1] : pathEdges[0];

    float distanceToEdgePoint = Vector3.Distance(trapObject.transform.position, pathEdge);
    if (distanceToEdgePoint < limitDistanceToEdge)
    {
      returningPath = !returningPath;
      StartCoroutine(TrapCooldown(delayMoveToNextPoint));
    }

    float currentSpeed = speed;
    if (useDeceleration)
    {
      float deceleration = 1f - (distanceToEdgePoint / Vector3.Distance(pathEdges[0], pathEdges[1]));
      currentSpeed = Mathf.Lerp(speed * maxDecceleration, speed * maxAcceleration, deceleration);
    }

    trapObject.transform.position = Vector3.MoveTowards(trapObject.transform.position, pathEdge, currentSpeed * Time.deltaTime);
  }
  #endregion

  #region ProjectileShooter
  private void ProjectileShooter()
  {
    GameObject arrowPrefab = Instantiate(projectile, transform.position, Quaternion.identity, gameObject.transform);
    arrowPrefab.SetActive(true);
    Rigidbody2D rb = arrowPrefab.GetComponent<Rigidbody2D>();
    rb.AddForce(shootDirection * shootForce, ForceMode2D.Impulse);
    StartCoroutine(TrapCooldown(shootCooldown));
  }
  #endregion

  private IEnumerator TrapCooldown(float cooldown)
  {
    isInCooldown = true;
    yield return new WaitForSeconds(cooldown);
    isInCooldown = false;
  }

  public enum TrapType
  {
    MovementHazard,
    ProjectileShooter,
    LandMine,
  }
}
