using System;
using System.Collections;
using System.Collections.Generic;
using IPlayerState;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  //public static event Action PlayerDefeatedEvent;
  private PlayerStateController _playerStateController;
  private GameObject player;
  private CapsuleCollider2D playerCol;
  private AudioManager _audioManager;

  private void Awake()
  {
    player = GameObject.FindWithTag("Player");
    _playerStateController = player.GetComponent<PlayerStateController>();
    lastCheckpointPosition = player.transform.position;
    playerCol = player.GetComponent<CapsuleCollider2D>();

    _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    _audioManager.StartMusicHandler();
  }
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.R))
    {
      string currentSceneName = SceneManager.GetActiveScene().name;
      SceneManager.LoadScene(currentSceneName);
    }

    //HandleSlowOnTime();
  }

  #region playerDeath and Checkpoints
  private Vector2 lastCheckpointPosition;
  private int currentCheckpointIndex;

  public void ReturnToCheckPoint(float respawnDelay)
  {
    Invoke(nameof(TeleportToCheckpoint), respawnDelay);
  }

  private void TeleportToCheckpoint()
  {
    player.transform.position = lastCheckpointPosition;
  }

  public void RegisterNewCheckpoint(int cpIndex, Vector2 pos)
  {
    if (cpIndex < currentCheckpointIndex)
      return;

    lastCheckpointPosition = pos;
    currentCheckpointIndex = cpIndex;
  }
  #endregion

  public void ShakeCamera(float shakeIntensity, float shakeDuration)
  {
    CinemachineShake.Instance.ShakeCamera(shakeIntensity, shakeDuration);
  }

  #region slow on time
    private float slowIntensity;
    private float slowdownTransitionLength;
    private bool timeSlowed;
    private void HandleSlowOnTime()
    {
      if (timeSlowed)
      {
        Time.timeScale = Mathf.Max(slowIntensity, 0.0001f);
        Time.fixedDeltaTime = Time.timeScale * .02f;
        return;
      }

      if (Time.timeScale == 1) // if time is normal theres no need to do anything
        return;

      Time.timeScale += 1f / slowdownTransitionLength * Time.unscaledDeltaTime;
      Time.timeScale = Math.Clamp(Time.timeScale, 0f, 1f);
    }
    public void CreateTemporarySlow(float _slowIntensity, float _slowDuration, float _slowdownTransitionLength)
    {
      timeSlowed = true;
      slowIntensity = _slowIntensity;
      slowdownTransitionLength = _slowdownTransitionLength > 0? _slowdownTransitionLength : 1;

      Invoke(nameof(EndSlow), _slowDuration * _slowIntensity);
    }

    private void EndSlow()
    {
      timeSlowed = false;
    }

    public bool TestIfItsOnScreen(Vector2 objectPosition, float limit)
    {
      Vector2 _vp = Camera.main.WorldToViewportPoint(objectPosition);
      return _vp.x >= 0f && _vp.x <= limit && _vp.y >= 0f && _vp.y <= limit;
    }
  #endregion
}
