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

  [SerializeField] private float delayToRespawn; // separate it as a solo script

  private void Awake()
  {
    player = GameObject.FindWithTag("Player");
    _playerStateController = player.GetComponent<PlayerStateController>();
    lastCheckPointPosition = player.transform.position;
  }
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.R))
    {
      string currentSceneName = SceneManager.GetActiveScene().name;
      SceneManager.LoadScene(currentSceneName);
    }

    HandleSlowOnTime();
  }

  #region playerDeath and Checkpoints
  private Vector2 lastCheckPointPosition;
  public void PlayerDefeated()
  {
    _playerStateController.isDefeated = true;
    Invoke(nameof(ReturnPlayerToGame), delayToRespawn);
  }

  private void ReturnPlayerToGame()
  {
    player.transform.position = lastCheckPointPosition;
    _playerStateController.isDefeated = false;
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
  #endregion
}
