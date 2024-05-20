using System;
using TarodevController;
using UnityEngine;
public class RangedEnemyCreature : EnemyCreature
{
  [SerializeField] private GameObject projectilePrefab;
  [SerializeField] private float delayToShootProjectile; // to get the animation timing correctly
  [SerializeField] private Vector2 projectileSpawnPosition;

  private EnemyAI _enemyAI;
  private Vector2 projectilePosition;

    #if UNITY_EDITOR // it draws the projectile spawn position to better visualize it
    private void OnDrawGizmos()
    {
      if (projectileSpawnPosition == null)
        return;

      Gizmos.color = Color.green;

      if (Application.isPlaying)
        Gizmos.DrawSphere(projectilePosition, .2f);
      else
        Gizmos.DrawSphere(projectileSpawnPosition + (Vector2)transform.position, .2f);
    }
  #endif

  protected override void Awake()
  {
    base.Awake();
    _enemyAI = GetComponentInParent<EnemyAI>();
  }

  protected override void Update()
  {
    base.Update();

    updateProjecTileSpawnPosition();
  }

  protected virtual void updateProjecTileSpawnPosition()
  {
    if (sprRenderer.flipX)
      projectilePosition = rb.position - projectileSpawnPosition;
    else
      projectilePosition = rb.position + projectileSpawnPosition;
  }

  public override void HandleAttack()
  {
    base.HandleAttack();
    rb.velocity = Vector2.zero;
  }

  protected override void Attack()
  {
    base.Attack();

    Invoke(nameof(shootProjectile), delayToShootProjectile);
  }

  protected override void AttackEnd()
  {
    base.AttackEnd();

    rb.velocity = Vector2.zero;
  }

  protected override void InvertSprite() // so it always face the target
  {
    if (_enemyAI.aIState == EnemyAI.AIState.Patroling) // it shouldn't face the player if its not aware of him yet
      return;

    if (isAttacking) // it shouldn't change directions while attacking
      return;

    Transform target = _playerStateController.transform;
    float lookDirection = Math.Abs(target.position.x) - Math.Abs(rb.position.x);

    sprRenderer.flipX = lookDirection < 0;
  }

  private void shootProjectile()
  {
    Vector2 shootDirection = sprRenderer.flipX ? Vector2.right : Vector2.left;

    GameObject arrowInstance = Instantiate(projectilePrefab, projectilePosition, Quaternion.identity, transform);
    if (sprRenderer.flipX)
      arrowInstance.transform.rotation = Quaternion.Euler(0, 0, 180);

    arrowInstance.SetActive(true);
    Rigidbody2D arrowRB = arrowInstance.GetComponent<Rigidbody2D>();
    arrowRB.AddForce(shootDirection * attackSpeed, ForceMode2D.Impulse);
  }
}
