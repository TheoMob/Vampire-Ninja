using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using IPlayerState;
using static IPlayerState.PlayerStateController;

public class PlayerCombatController : MonoBehaviour
{
  private PlayerStateController _stateController;
  private int playerMaxHealth = 1;
  public int playerHealth;

  private void Awake()
  {
    _stateController = GetComponent<PlayerStateController>();
    playerHealth = playerMaxHealth;
  }

  public void PlayerLosesHealth(int dmgPoints)
  {
    playerHealth -= dmgPoints;

    if (playerHealth <= 0)
      PlayerDeath();
  }

  private void PlayerDeath()
  {
    _stateController.OnPlayerDefeated();
  }

  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.CompareTag("Hazards"))
    {
      PlayerLosesHealth(1);
    }
  }

}
