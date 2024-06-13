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
        Invoke(nameof(spikeActivation), trapAttackDelay);

        _audioManager.Play("TrapReady");
    }

    private void spikeActivation()
    {
        trapCollider.enabled = true;;
        trapAnimator.Play(TRAP_ACTIVATED);
        Invoke(nameof(spikeDeactivation), trapAttackDuration);

        _audioManager.Play("Spike2");
    }

    private void spikeDeactivation()
    {
        trapCollider.enabled = false;
        StartCoroutine(TrapCooldown());
        trapAnimator.Play(TRAP_IDLE);
    }
}
