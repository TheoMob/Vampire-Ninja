using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LockVirtualCamera : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        virtualCamera = transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") == false)
            return;

        virtualCamera.Priority = 11;
    }
    private void OnTriggerExit2D(Collider2D col) 
    {
        if (col.CompareTag("Player") == false)
            return;

        virtualCamera.Priority = 9;
    }

}
