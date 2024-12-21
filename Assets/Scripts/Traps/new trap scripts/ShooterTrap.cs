using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShooterTrap : MonoBehaviour
{
    private AudioManager g_audioManager => AudioManager.instance;
    private Projectile[] _projectilePool = new Projectile[3];
    private int _currentPoolIndex = 0;
    private Animator _anim;

    [Header("Dispenser variables")]
    private bool _isInCooldown;
    [SerializeField] private bool _activated;
    [SerializeField] private float _shootForce;
    [SerializeField] private Vector2 _shootPosition = new Vector2(.125f, .125f);
    [SerializeField] private float _initialDelay; // this is so you can have some offset across other nearby traps
    [SerializeField] private float _delayBetweenShots;

    [Header("Projectile variables")]
    [SerializeField] private float _projectileMaxDuration = 5f;
    [SerializeField] private float _delayToFadeAfterColliding = 1f;

    private bool _isVisible = true;

    private void Start()
    {
        _anim = GetComponent<Animator>();

        for (int i = 0; i < _projectilePool.Length; i++) // initializes every object from the pool
        {
            GameObject projectile = transform.GetChild(i).gameObject;

            _projectilePool[i].obj = projectile;
            _projectilePool[i].initialPosition = projectile.transform.position;
            _projectilePool[i].rb = projectile.GetComponent<Rigidbody2D>();
            _projectilePool[i].collider = projectile.GetComponent<Collider2D>();
            _projectilePool[i].spr = projectile.GetComponent<SpriteRenderer>();

            _projectilePool[i].collider.enabled = false;
            _projectilePool[i].spr.enabled = false;
        }

        if (_initialDelay > 0)
            StartCoroutine(HandleInitialDelay());
    }

    private void Update()
    {
        if (!_activated)
            return;

        if (_isInCooldown)
            return;

        if (!_isVisible) // this is just so traps still keep themselves in sync
        {
            StartCoroutine(HandleTrapCooldown());
            return;
        }

        ShootProjectile();  
    }

    private void ShootProjectile()
    {
        Projectile projectile = _projectilePool[_currentPoolIndex];

        Vector2 shootPos = (Vector2)transform.position + (_shootPosition * transform.up);
        projectile.obj.transform.position = shootPos;
        //projectile.collider.enabled = true;
        projectile.spr.enabled = true;

        projectile.rb.AddForce(transform.up * _shootForce, ForceMode2D.Impulse);

        g_audioManager.Play("Kunai1", true, transform.position);
        _anim.Play("TrapAttack");

        StartCoroutine(DelayToShotToGetCollision(projectile.collider));
        StartCoroutine(HandleTrapCooldown());
        StartCoroutine(ReturnProjectileToPool());
    }

    IEnumerator DelayToShotToGetCollision(Collider2D col) // if the trap is too close to the ground, it can immediatelly count as a collision, this is so it doesnt happen
    {
        yield return new WaitForSeconds(0.1f);
        col.enabled = true;
    }

    IEnumerator HandleInitialDelay()
    {
        _isInCooldown = true;
        yield return new WaitForSeconds(_initialDelay);
        _isInCooldown = false;
    }

    IEnumerator HandleTrapCooldown() // visibility tests are being made so traps keep in sync even off screen, while not playing animations and etc
    {
        _isInCooldown = true;
        yield return new WaitForSeconds(_delayBetweenShots/2);

        if (_isVisible)
            _anim.Play("TrapReady");

        yield return new WaitForSeconds(_delayBetweenShots/2);
        _isInCooldown = false;

        if (_isVisible)
        {
            _currentPoolIndex = _currentPoolIndex + 1;
            if (_currentPoolIndex >= _projectilePool.Length)
                _currentPoolIndex = 0;
        }
    }

    IEnumerator ReturnProjectileToPool() // returns the projectile back to the pool
    {
        Projectile projectile = _projectilePool[_currentPoolIndex];

        yield return new WaitForSeconds(_projectileMaxDuration);
        projectile.obj.transform.position = projectile.initialPosition;
        projectile.rb.velocity = Vector2.zero;
        projectile.collider.enabled = false;
        projectile.spr.color = Color.white;
        projectile.spr.enabled = false;
    }

    #region Projectile Handler

    public void OnCollideWithWall(SpriteRenderer projectileSpr)
    {
        g_audioManager.Play("Kunai2", true, projectileSpr.transform.position);
        StartCoroutine(FadingEffect(projectileSpr));
    }

    IEnumerator FadingEffect(SpriteRenderer projectileSpr)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.1f);
            Color newColor = projectileSpr.color;
            newColor.a = newColor.a - .1f;
            projectileSpr.color = newColor;

            if (newColor.a <= 0)
                break;
        }
    }

    private struct Projectile
    {
        public GameObject obj;
        public Vector2 initialPosition;
        public Rigidbody2D rb;
        public Collider2D collider;
        public SpriteRenderer spr;
    }
    #endregion

    #region Visibility
    public void OnBecameVisible() 
    {
        _isVisible = true;
    }

    public void OnBecameInvisible() 
    {
        _isVisible = false;
    }
    #endregion
}
