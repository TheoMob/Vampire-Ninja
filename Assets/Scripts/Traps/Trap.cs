using System.Collections;
using UnityEngine;
using NaughtyAttributes;

public class Trap : MonoBehaviour
{
    [SerializeField] protected bool trapDisabled = false;
    [SerializeField] protected Direction attackDirection;
    [SerializeField] protected bool externalActivation = false;
    [HideIf(nameof(externalActivation))] [SerializeField] protected float delayToStartWorking;
    [SerializeField] protected float trapAttackDelay;
    [HideIf(nameof(externalActivation))] [SerializeField] protected float trapCooldown;
    protected AudioManager _audioManager => AudioManager.instance;
    protected GameManager _gameManager;
    protected bool isInCooldown = false;
    protected Animator trapAnimator;

    protected const string TRAP_IDLE = "TrapIdle";
    protected const string TRAP_READY = "TrapReady";
    protected const string TRAP_ACTIVATED = "TrapAttack";
    [SerializeField] protected Transform playerTransform;

    protected virtual void Awake()
    {
        trapAnimator = GetComponent<Animator>();
        _gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void OnEnable()
    {
        if (externalActivation)
            return;
            
        StartCoroutine(InitialDelay());
        isInCooldown = false;
    }

    protected virtual void OnDisable() 
    {
        StopAllCoroutines();
    }

    protected virtual void FixedUpdate()
    {
        if (trapDisabled || isInCooldown || externalActivation)
            return;
        
        TrapFunction();
    }

    protected virtual void TrapFunction()
    {
        // code of the working trap, since this is a base script there is no function associated to it
    }

    protected virtual Vector2 GetTrapDirection()
    {
        switch (attackDirection)
        {
            case Direction.Up:
                return Vector2.up;
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
        }

        Debug.LogError("No valid direction detected");
        return Vector2.up;
    }


    protected virtual IEnumerator InitialDelay()
    {
        trapDisabled = true;
        yield return new WaitForSeconds(delayToStartWorking);
        trapDisabled = false;
    }
    protected virtual IEnumerator TrapCooldown()
    {
        isInCooldown = true;
        yield return new WaitForSeconds(trapCooldown);
        isInCooldown = false;
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
