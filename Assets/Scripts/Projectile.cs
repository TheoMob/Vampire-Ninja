using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private Rigidbody2D rb;
  private BoxCollider2D col;
  private SpriteRenderer sprRenderer;
  [SerializeField] private float disappearAfterCollidingTime = 2f;

  private void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    col = GetComponent<BoxCollider2D>();
    sprRenderer = GetComponent<SpriteRenderer>();
  }

  private void OnTriggerEnter2D(Collider2D otherCollider)
  {
    if (otherCollider.gameObject.tag == "Player")
    {
      Destroy(gameObject);
      return;
    }

    if (otherCollider.gameObject.tag == "Ground" || otherCollider.gameObject.tag == "Wall")
    {
      Vector3 direction = getDirection()*0.25f;
      rb.velocity = Vector2.zero;
      transform.position += direction;
      col.enabled = false;
      InvokeRepeating("ProjectileDisappear", disappearAfterCollidingTime - 0.5f, .1f);
    }
  }

  private void ProjectileDisappear()
  {
    float rateToDisappear = 0.5f / (1 / 0.1f);
    Color newColor = sprRenderer.color;
    newColor.a -= rateToDisappear;
    sprRenderer.color = newColor;

    if (sprRenderer.color.a <= 0.1f)
    {
      Debug.Log("DESTROOOOOOOOOOOOY");
      Destroy(gameObject);
    }
  }

  private Vector3 getDirection()
  {
    if (Math.Abs(rb.velocity.x) >= Math.Abs(rb.velocity.y))
    {
      if (Math.Sign(rb.velocity.x) >= 0)
        return Vector3.right;
      else
        return Vector3.left;
    }
    else
    {
      if (Math.Sign(rb.velocity.y) >= 0)
        return Vector3.up;
      else
        return Vector3.down;
    }
  }
}
