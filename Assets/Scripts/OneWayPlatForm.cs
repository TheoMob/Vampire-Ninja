using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class OneWayPlatForm : MonoBehaviour
{
  private PlayerController _playerController;
  private PlatformEffector2D platformEffector;

  private void Awake()
  {
    platformEffector = GetComponent<PlatformEffector2D>();
    //_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.collider.CompareTag("Player"))
      _playerController = collision.gameObject.GetComponent<PlayerController>();
  }

  private void OnCollisionStay2D(Collision2D coll)
  {
    if (_playerController == null)
      return;


    if (_playerController._crouching)
    {
      platformEffector.rotationalOffset = 180;
      _playerController = null;
    }
  }

  private void OnCollisionExit2D(Collision2D coll)
  {
    _playerController = null;
    platformEffector.rotationalOffset = 0;
  }
}
