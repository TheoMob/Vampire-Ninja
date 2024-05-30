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
    if (col.CompareTag("Hazards"))
    {
      PlayerLosesHealth(1);
    }
  }

}
