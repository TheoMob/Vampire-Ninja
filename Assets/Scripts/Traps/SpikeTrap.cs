using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : Trap
{
    [SerializeField] private float trapAttackDuration;
    [SerializeField] private bool activateOnStep = false;
    private BoxCollider2D trapCollider;

    protected override void Awake()
    {
        base.Awake();
        trapCollider = GetComponent<BoxCollider2D>();
        trapCollider.enabled = false;
    }

    protected override void FixedUpdate()
    {
        if(isInCooldown || trapDisabled)
            return;

        if (activateOnStep)
        {
            bool steppingOnTrap = Physics2D.BoxCast(trapCollider.bounds.center, trapCollider.size, 0, GetTrapDirection(), trapCollider.size.y / 2, LayerMask.GetMask("Player"));
            if (steppingOnTrap == false)
                return;
        }
        TrapFunction();
    }
    protected override void TrapFunction()
    {
        isInCooldown = true;
        trapAnimator.Play(TRAP_READY);

        _audioManager.Play("TrapReady", true, transform.position);
        
        StartCoroutine(SpikeActivationHandler());
    }

    IEnumerator SpikeActivationHandler()
    {
        yield return new WaitForSeconds(trapAttackDelay);
        trapCollider.enabled = true;
        trapAnimator.Play(TRAP_ACTIVATED);

        _audioManager.Play("Spike2", true, transform.position);

        yield return new WaitForSeconds(trapAttackDuration);
        trapCollider.enabled = false;
        trapAnimator.Play(TRAP_IDLE);

        StartCoroutine(TrapCooldown());
    }
}
