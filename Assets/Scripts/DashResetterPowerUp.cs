using System.Collections;
using System.Collections.Generic;
using IPlayerState;
using TarodevController;
using Unity.VisualScripting;
using UnityEngine;

public class DashResetterPowerUp : MonoBehaviour
{
  private PlayerStateController _playerStateController;
  [SerializeField] private float delayToReappear = 2f;
  private SpriteRenderer sprRenderer;
  private BoxCollider2D col;
  private Animator anim;

  private void Awake()
  {
    _playerStateController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();
    sprRenderer = GetComponent<SpriteRenderer>();
    col = GetComponent<BoxCollider2D>();
    anim = GetComponent<Animator>();
  }

  private void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.tag != "Player")
      return;

    SetComponentsActive(false);
    StartCoroutine(Reappear());
  }

  IEnumerator Reappear()
  {
    yield return new WaitForSeconds(delayToReappear);
    SetComponentsActive(true);
  }

  private void SetComponentsActive(bool setActive)
  {
    sprRenderer.enabled = setActive;
    col.enabled = setActive;
    anim.enabled = setActive;
  }
}
