using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using IPlayerState;
using static IPlayerState.PlayerStateController;

public class PlayerCombatController : MonoBehaviour
{
  private PlayerStateController _stateController;
  private GameManager _gameManager;
  private Rigidbody2D rb;

  private int playerMaxHealth = 1;
  public int playerHealth;

  [Header("Attack variables")]
  public bool isAttacking;
  [SerializeField] private float attackDuration = 1f;
  [SerializeField] private float attackCoolDown = 1f;
  private bool isAttackOnCooldown = false;

  private void Awake()
  {
    _stateController = GetComponent<PlayerStateController>();
    rb = GetComponent<Rigidbody2D>();

    _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

    playerHealth = playerMaxHealth;
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.J))
    {
      HandleAttack();
    }
  }

  private void HandleAttack()
  {
    if (CheckIfCanAttack() == false)
      return;

    rb.velocity = Vector2.zero;

    StartCoroutine(HandleAttackDuration());
    StartCoroutine(HandleAttackCooldown());
  }

  IEnumerator HandleAttackDuration()
  {
    isAttacking = true;
    yield return new WaitForSeconds(attackDuration);
    isAttacking = false;
  }

  IEnumerator HandleAttackCooldown()
  {
    isAttackOnCooldown = true;
    yield return new WaitForSeconds(attackCoolDown);
    isAttackOnCooldown = false;
  }

  private bool CheckIfCanAttack()
  {
    if (_stateController.GetCurrentState() != PlayerState.Idle || isAttackOnCooldown) // adjust later to accept air attack
      return false;

    return true;
  }

  public void PlayerLosesHealth(int dmgPoints)
  {
    playerHealth -= dmgPoints;

    if (playerHealth <= 0)
      PlayerDeath();
  }

  private void PlayerDeath()
  {
    _gameManager.PlayerDefeated();
    _stateController.isDefeated = true;
  }

  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.tag == "Hazards")
    {
      PlayerLosesHealth(1);
    }
  }

}
