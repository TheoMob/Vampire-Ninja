using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : Trap
{
    [SerializeField] protected float shootForce;
    [SerializeField] protected GameObject projectilePrefab;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void TrapFunction()
    {
        isInCooldown = true;
        trapAnimator.Play(TRAP_READY);
        StartCoroutine(ShootArrow());
    }

    public void ForceActivation(float newShootForce)
    {
        if (newShootForce > 0)
            shootForce = newShootForce;
            
        TrapFunction();
    }

    [SerializeField] private Vector2 projectileOffset;

    IEnumerator ShootArrow()
    {
        yield return new WaitForSeconds(trapAttackDelay);
        trapAnimator.Play(TRAP_ACTIVATED);

        Vector2 direction = GetTrapDirection();
        Vector2 arrowOffset = -0.1f * direction; // this is so the arrow dont spawn exactly in the middle, but rather on the exit of the trap, this is related to the sprite, so it needs to change in case the sprite does
        Vector2 spawnPosition = arrowOffset + (Vector2)transform.position + projectileOffset;

        GameObject arrow = Instantiate(projectilePrefab, spawnPosition, transform.rotation, null);
        arrow.SetActive(true);

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(direction * shootForce, ForceMode2D.Impulse);
        
        _audioManager.Play("Kunai1", true, transform.position);

        StartCoroutine(TrapCooldown());
    }
}
