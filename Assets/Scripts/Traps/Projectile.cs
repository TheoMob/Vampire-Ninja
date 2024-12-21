using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  [SerializeField] private bool _isTrap = true;
  private ShooterTrap _shooterTrap;
  private BoxCollider2D _col;
  private SpriteRenderer _spr;
  private Rigidbody2D _rb;

  private void Start()
  {
    if (_isTrap)
      _shooterTrap = transform.parent.GetComponent<ShooterTrap>();

    _col = GetComponent<BoxCollider2D>();
    _spr = GetComponent<SpriteRenderer>();
    _rb = GetComponent<Rigidbody2D>();
  }

  private void OnTriggerEnter2D(Collider2D otherCollider)
  {
    if (otherCollider.gameObject.tag == "Player")
    {
      _col.enabled = false;
      _spr.enabled =false;
      _rb.velocity = Vector2.zero;
      return;
    }

    if (otherCollider.gameObject.tag == "Ground" || otherCollider.gameObject.tag == "Wall")
    {
      if (_isTrap)
      {
        _rb.velocity = Vector2.zero;
        _col.enabled = false;
        _shooterTrap.OnCollideWithWall(_spr);
      }
    }
  }
}
