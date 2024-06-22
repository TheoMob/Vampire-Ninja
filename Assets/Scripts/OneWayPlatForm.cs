using System.Collections;
using System.Collections.Generic;
using IPlayerState;
using static IPlayerState.PlayerStateController;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
  private const string PLATFORM_LAYER = "OneWayPlatform";
  private const string PLAYER_LAYER = "Player";
  private PlayerStateController _playerStateController;
  private BoxCollider2D platformCollider;

  private void Awake()
  {
    _playerStateController =  GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();
    platformCollider = GetComponent<BoxCollider2D>();
  }

  private void Update()
  {
    // if (!Input.GetButtonDown("Jump") || !Input.GetButton("Jump"))
    //   return;
      
    bool isTouchingPlatform = Physics2D.OverlapBox(platformCollider.bounds.center, platformCollider.size, 0, LayerMask.GetMask(PLAYER_LAYER));
    bool isPlatformCollisionOff = Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer(PLAYER_LAYER), LayerMask.NameToLayer(PLATFORM_LAYER));
    bool turnPlatformOff = _playerStateController.GetFrameInput().Move.y < 0;

    if (isTouchingPlatform && !isPlatformCollisionOff && turnPlatformOff)
    {
      Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(PLAYER_LAYER), LayerMask.NameToLayer(PLATFORM_LAYER), true);
      InvokeRepeating("TestIfisTouchingPlatform", 0f, 0.1f);
    }
  }

  private void TestIfisTouchingPlatform() // this exist so each individual platform test it's on collision, if it was on update every instance of oneWayPlatform would try to change the layer individually, creating a buggy mess
  {
    bool isTouchingPlatform = Physics2D.OverlapBox(platformCollider.bounds.center, platformCollider.size, 0, LayerMask.GetMask(PLAYER_LAYER));
    if (!isTouchingPlatform)
    {
      Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(PLAYER_LAYER), LayerMask.NameToLayer(PLATFORM_LAYER), false);
      CancelInvoke("TestIfisTouchingPlatform");
    }
  }
}
