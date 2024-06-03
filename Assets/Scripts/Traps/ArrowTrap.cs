using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : Trap
{
    [SerializeField] private float shootForce;
    [SerializeField] GameObject projectilePrefab;
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
        Invoke(nameof(ShootArrow), delayToStartWorking);
    }

    private void ShootArrow()
    {
        trapAnimator.Play(TRAP_ACTIVATED);

        Vector2 direction = GetTrapDirection();
        Vector2 arrowOffset = -0.1f * direction; // this is so the arrow dont spawn exactly in the middle, but rather on the exit of the trap, this is related to the sprite, so it needs to change in case the sprite does
        Vector2 spawnPosition = arrowOffset + (Vector2)transform.position;

        GameObject arrow = Instantiate(projectilePrefab, spawnPosition, transform.rotation, gameObject.transform);
        arrow.SetActive(true);

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(direction * shootForce, ForceMode2D.Impulse);

        StartCoroutine(TrapCooldown());
    }
}
