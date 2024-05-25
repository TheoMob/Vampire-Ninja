using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO.Pipes;
using IPlayerState;
using static IPlayerState.PlayerStateController;
using Unity.Mathematics;

public class BaseCreature : MonoBehaviour
{
    protected PlayerStateController _playerStateController;
    protected PlayerCombatController _playerCombat;

    //components
    protected SpriteRenderer sprRenderer;
    protected Collider2D hitDetectionCollider;
    protected Rigidbody2D rb;

    // combat and damage related
    [SerializeField] protected bool isImmortal;
    [HideIf("isImmortal")] [SerializeField] protected float enemyHealth = 1f;
    [SerializeField] protected bool isPassive;
    protected float enemyCurrentHealth;
    protected bool isDead;

    //animations
    Animator creatureAnimator;
    protected const string IDLE = "Idle";
    protected const string HIT_TAKEN = "HitTaken";
    protected const string DIE = "Die";

    protected virtual void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        _playerStateController = player.GetComponent<PlayerStateController>();
        _playerCombat = player.GetComponent<PlayerCombatController>();
        hitDetectionCollider = transform.GetChild(0).GetComponent<Collider2D>();
        
        sprRenderer = GetComponent<SpriteRenderer>();
        creatureAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        enemyCurrentHealth = enemyHealth;
    }

    protected virtual void InvertSprite()
    {
        if (math.abs(rb.velocity.x) < 0.1f)
        return;

        sprRenderer.flipX = rb.velocity.x > 0.1f ? true : false;
    }

    protected virtual void OnCreatureHit()
    {
        if (!isImmortal)
            enemyCurrentHealth = enemyCurrentHealth - 1;

        _playerStateController.ResetDash();
        
        if (enemyCurrentHealth <= 0)
        {
            OnCreatureDie();    
            return;
        }

        creatureAnimator.Play(HIT_TAKEN);
    }

    protected virtual void OnCreatureDie()
    {
        isDead = true;
        creatureAnimator.Play(DIE);
        float deathAnimationDuration = creatureAnimator.GetCurrentAnimatorStateInfo(0).length;
        hitDetectionCollider.enabled = false;
        
        Invoke(nameof(DestroyCreature), deathAnimationDuration);
    }

    protected virtual void DestroyCreature()
    {
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {   
        string colliderTag = col.gameObject.tag;

        if (colliderTag != "Player" && colliderTag != "PlayerAttackHitbox")
            return;
        
        bool playerIsAttacking = _playerStateController.GetCurrentState() == PlayerState.Dashing;

        if (colliderTag == "Player" && !playerIsAttacking && !isPassive)
        {
             _playerCombat.PlayerLosesHealth(1);
            return;
        }

        if (colliderTag == "PlayerAttackHitbox" && playerIsAttacking)
        {
            OnCreatureHit();
        }
    }
}
