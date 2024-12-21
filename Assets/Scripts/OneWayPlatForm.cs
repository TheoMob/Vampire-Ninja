using UnityEngine;
public class OneWayPlatform : MonoBehaviour
{
  GameObject _player;
  private CapsuleCollider2D _playerCol;
  private BoxCollider2D _platformCol;
  private bool _isTouchingPlatform;

  private void Start()
  {
    _player = GameObject.FindWithTag("Player");
    _playerCol = _player.GetComponent<CapsuleCollider2D>();
    _platformCol = GetComponent<BoxCollider2D>();
  }

  private void Update()
  {
    bool _isVisible = false;
    foreach(SpriteRenderer childSpr in GetComponentsInChildren<SpriteRenderer>())
    {
      if (childSpr.isVisible)
      {
        _isVisible = true;
        break;
      }
    }

    if (!_isVisible)
    {
      IgnoreCollision(false);
      return;
    }

    float bottomPlayerPos = _playerCol.bounds.center.y - _playerCol.bounds.extents.y; // gets the lowest Y from the capsuleCollider (the feet)
    float topPlayerPos = _playerCol.bounds.center.y + _playerCol.bounds.extents.y; // gets the highest Y from the capsuleCollider
    float topPlatformPos = _platformCol.bounds.center.y - _platformCol.bounds.extents.y;
    bool touchingPlatform = Physics2D.BoxCast(_platformCol.bounds.center, _platformCol.bounds.size, 0f, Vector2.up, .1f, LayerMask.GetMask("Player"));

    if (topPlayerPos <= topPlatformPos || !touchingPlatform) // if the player is completely below the platform, turn the collision on
    {
      IgnoreCollision(false);
      return;
    }

    if (bottomPlayerPos >= topPlatformPos && PlayerInputManager.FrameInput.Move.y < 0 && _isTouchingPlatform)
      IgnoreCollision(true);
  }

  private void IgnoreCollision(bool setActive)
  {  
    Physics2D.IgnoreCollision(_playerCol, _platformCol, setActive);
  }

  void OnCollisionEnter2D(Collision2D col)
  {
    if (col.gameObject.tag != "Player")
      return;
    
    _isTouchingPlatform = true;  
  }
  void OnCollisionExit2D(Collision2D col)
  {
    if (col.gameObject.tag != "Player")
      return;

    _isTouchingPlatform = false;
  }
}
