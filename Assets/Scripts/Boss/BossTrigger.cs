using System.Collections;
using System.Collections.Generic;
using IPlayerState;
using static IPlayerState.PlayerStateController;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class BossTrigger : MonoBehaviour
{
    private GameObject player;
    private PlayerStateController _playerStateController;
    private PlayerCombatController _playerCombatController;
    private AudioManager _audioManager;
    private BoxCollider2D col;
    private SpriteRenderer spr;
    private Animator anim;
    private bool vulnerable;
    void Start()
    {
        vulnerable = true;
        player = GameObject.FindWithTag("Player");
        _playerStateController = player.GetComponent<PlayerStateController>();
        _playerCombatController = player.GetComponent<PlayerCombatController>();

        _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
        
        col = GetComponent<BoxCollider2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    public bool bossHit = false;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        CheckColissionToPlayer(other);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckColissionToPlayer(other);
    }

    private void CheckColissionToPlayer(Collider2D other)
    {
        if (bossHit)
            return;
            
        bool isDashing = _playerStateController.GetCurrentState() == PlayerState.Dashing;

        if (other.CompareTag("PlayerAttackHitbox") && isDashing) // damage the boss
            triggerStart();
    }

    private void triggerStart()
    {
        anim.Play("SmokeAnimation");
        _audioManager.Play("Smoke3", false, Vector2.zero);
        StartCoroutine(StartBossFight());
    }

    public void ResetBoss()
    {
        gameObject.SetActive(true);
        col.enabled = true;
        spr.enabled = true;
        bossHit = false;
        anim.Play("Idle");
    }
    IEnumerator StartBossFight()
    {
        StopCoroutine(BossAppearLeft());

        bossHit = true;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y - .5f), 100);

        col.enabled = false;
        vulnerable = false;

        yield return new WaitForSeconds(.5f);
        spr.enabled = false;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y + .5f), 100);
        BossManager bossManager = GetComponentInParent<BossManager>();
        bossManager.StartFight();
    }

    public void StartBossFightByTime()
    {
        anim.Play("SoruBack");  
      _audioManager.Play("Soru", false, Vector2.zero);
      StartCoroutine(StartBossFight());
    }

    [SerializeField] private float windowToAttackBoss;
    [SerializeField] private float dashSpeed;
    
    public void BossAppear()
    {
        spr.enabled = true;
        col.enabled = true;
        bossHit = false;

        StartCoroutine(BossAppearLeft());
    }

    IEnumerator BossAppearLeft()
    {
        float facingDirection = Math.Sign(transform.position.x - player.transform.position.x);
        spr.flipX = facingDirection > 0 ? true : false;

        _audioManager.Play("Soru", false, Vector2.zero);
        anim.Play("Soru");
        yield return new WaitForSeconds(windowToAttackBoss);

        if (bossHit)
            yield break;

        _audioManager.Play("Soru", false, Vector2.zero);
        anim.Play("SoruBack");

        yield return new WaitForSeconds(0.2f);
        BossManager bossManager = GetComponentInParent<BossManager>();
        bossManager.StartFight();
        spr.enabled = false;
        col.enabled = false;
    }
}
