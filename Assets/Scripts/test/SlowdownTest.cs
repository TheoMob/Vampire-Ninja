using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowdownTest : MonoBehaviour
{
    [SerializeField] private float slowdownFactor = 0.05f;
    [SerializeField] private float slowdownTransitionLength = 2f;
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * .02f;
        }
        else
        {
            Time.timeScale += 1f/ slowdownTransitionLength * Time.unscaledDeltaTime;
            Time.timeScale = Math.Clamp(Time.timeScale, 0f, 1f);
        }
    }
}
