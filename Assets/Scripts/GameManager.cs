using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  public static event Action PlayerDefeatedEvent;
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.R))
    {
      string currentSceneName = SceneManager.GetActiveScene().name;
      SceneManager.LoadScene(currentSceneName);
    }
  }

  public void PlayerDefeated()
  {
    if (PlayerDefeatedEvent != null)
      PlayerDefeatedEvent();
  }

  public void ShakeCamera(float shakeIntensity, float shakeDuration)
  {
    CinemachineShake.Instance.ShakeCamera(shakeIntensity, shakeDuration);
  }

  public void SlowTime(float slowIntensity, float slowDuration)
  {
    if (Time.timeScale != 1)
      return;

    Time.timeScale = Mathf.Max(1 - slowIntensity, 0.1f);

    Invoke(nameof(ReturnTimeToNormal), slowDuration);
  }

  private void ReturnTimeToNormal()
  {
    Time.timeScale = 1;
  }
}
