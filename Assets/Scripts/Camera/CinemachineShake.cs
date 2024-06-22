using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
  public static CinemachineShake Instance { get; private set; }
  private CinemachineVirtualCamera virtualCamera;
  private float shakeTimer;

  private void Awake()
  {
    Instance = this;
    virtualCamera = GetComponent<CinemachineVirtualCamera>();
  }

  public void ShakeCamera(float intensity, float duration)
  {
    CinemachineBasicMultiChannelPerlin channelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    channelPerlin.m_AmplitudeGain = intensity;
    shakeTimer = duration;
  }

  private void Update()
  {
    if (shakeTimer > 0)
    {
      shakeTimer -= Time.deltaTime;
      if (shakeTimer <= 0f)
      {
        CinemachineBasicMultiChannelPerlin channelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        channelPerlin.m_AmplitudeGain = 0f;
      }
    }
  }
}
