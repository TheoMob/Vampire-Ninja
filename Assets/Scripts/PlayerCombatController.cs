using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
  private int playerMaxHealth = 1;
  public int playerHealth;
  private BoxCollider2D _hitCol;
  private PlayerController _playerController;
  private GameManager _gameManager;

  private void Awake()
  {
    _hitCol = GetComponent<BoxCollider2D>();
    _hitCol.enabled = false;
    _playerController = GetComponent<PlayerController>();
    _gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

    playerHealth = playerMaxHealth;
  }

  private void Update()
  {
    CheckForDashAttack();
  }

  private void CheckForDashAttack()
  {
    bool isAttacking = _playerController.dashState == DashState.Dashing;

    _hitCol.enabled = isAttacking;
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
  }

  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.tag == "Hazards")
    {
      PlayerLosesHealth(1);
    }
  }

}
