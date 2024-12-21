using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
public class SpikesTrap : MonoBehaviour
{
    private AudioManager g_audioManager => AudioManager.instance;
    [SerializeField] private bool _activated = true;
    [SerializeField] private bool _activatedOnStep = false;
    [ShowIf(nameof(_activatedOnStep))][SerializeField] private float _delayToActivate;
    [SerializeField] private float _cooldown;
    [SerializeField] private bool _isInCooldown; // remove te serialize afterwards
    [SerializeField] private float _activatedDuration;

    private BoxCollider2D _col;
    private Animator _anim;
    private SpriteRenderer _spr;
    void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!_activated)
            return;
        if (_isInCooldown)
            return;

        if (!_spr.isVisible && !_activatedOnStep) // this is just so traps still keep themselves in sync
        {
            StartCoroutine(HandleCooldown());
            return;
        }

        if (_activatedOnStep)
        {
            StepDetection();
            return;
        }

        ActivateTrap();
    }

    private void StepDetection()
    {
        bool steppingOnTrap = Physics2D.BoxCast(_col.bounds.center, _col.size, 0, transform.up, _col.size.y / 2, LayerMask.GetMask("Player"));
        if (steppingOnTrap)
            ActivateTrap();
    }

    private void ActivateTrap()
    {
        StartCoroutine(HandleDelayToActivate());
        StartCoroutine(HandleActiveDuration());
        StartCoroutine(HandleCooldown());
    }

    IEnumerator HandleDelayToActivate()
    {
        yield return new WaitForSeconds(_delayToActivate);
        _col.enabled = true;
        _anim.Play("TrapAttack");
        g_audioManager.Play("Spike2", true, transform.position);
    }

    IEnumerator HandleActiveDuration() // retract the spikes after a certain amount of time
    {
        yield return new WaitForSeconds(_activatedDuration);
        _col.enabled = false;
        _anim.Play("TrapIdle");
    }
    IEnumerator HandleCooldown()
    {
        _isInCooldown = true;
        yield return new WaitForSeconds(_cooldown/2);
        _anim.Play("TrapReady");
        g_audioManager.Play("TrapReady", true, transform.position);

        yield return new WaitForSeconds(_cooldown/2);
        _isInCooldown = false;
    }
}
