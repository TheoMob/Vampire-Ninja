using UnityEngine;

public class MeleeEnemyCreature : EnemyCreature
{
  protected override void Awake()
  {
    base.Awake();
  }

  public override void HandleAttack()
  {
    base.HandleAttack();

    sprRenderer.color = Color.yellow;
  }

  protected override void Attack()
  {
    base.Attack();

    float direction = sprRenderer.flipX ? 1 : -1;
    sprRenderer.color = Color.red;
    rb.velocity = new Vector2(attackSpeed * direction, 0);
  }

  protected override void AttackEnd()
  {
    base.AttackEnd();

    rb.velocity = Vector2.zero;
    sprRenderer.color = Color.white;
  }

  protected override void OnCreatureHit()
  {
    base.OnCreatureHit();
  }

  protected override void Reappear()
  {
    base.Reappear();
  }
}
