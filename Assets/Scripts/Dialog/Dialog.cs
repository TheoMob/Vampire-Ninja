using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IPlayerState;
using System;
using Cinemachine;

public class Dialog : MonoBehaviour
{
    [SerializeField] private string text = "Não temos muito tempo coff... coff... Vá  antes que seja tarde...";
    [SerializeField] private GameObject dialogBox;
    private SpriteRenderer dialogBoxImage;
    private TextMeshPro textBox;
    [SerializeField] private float textDelay;
    [SerializeField] private float dialogDuration;

    private PlayerStateController _stateController;
    private bool dialogAlreadyPlayed = false;

    private bool appearing = true;

    private CinemachineVirtualCamera closeUpCamera;

    private void Start()
    {
        _stateController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();

        dialogBox = GameObject.FindGameObjectWithTag("DialogBox");
        dialogBoxImage = dialogBox.GetComponent<SpriteRenderer>();

        textBox = dialogBox.transform.GetChild(0).GetComponent<TextMeshPro>();
        
        dialogBox.SetActive(false);
    }

    IEnumerator TypeWriterEffect()
    {
        for (int i = 0; i < text.Length; i++)
        {
            textBox.text = text.Substring(0, i + 1);
            yield return new WaitForSeconds(textDelay);
        }
    }

    IEnumerator EndDialog()
    {
        yield return new WaitForSeconds(dialogDuration);
        //dialogBox.SetActive(false);
        //_stateController.cantMove = false;
        InvokeRepeating(nameof(ImageDisappear), 0, .1f); 

        yield return new WaitForSeconds(1);
        _stateController.cantMove = false;
        closeUpCamera.Priority = 09;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (dialogAlreadyPlayed || !other.CompareTag("Player"))
            return;

        StartCoroutine(nameof(StartDialog));
    }

    IEnumerator StartDialog()
    {
        closeUpCamera = GameObject.FindGameObjectWithTag("CameraLockedOnLee").GetComponent<CinemachineVirtualCamera>();
        closeUpCamera.Priority = 11;
        _stateController.cantMove = true;
        dialogAlreadyPlayed = true;

        yield return new WaitForSeconds(1);

        dialogBox.SetActive(true);
        StartCoroutine(nameof(TypeWriterEffect));
        StartCoroutine(nameof(EndDialog));
    }

    private void ImageDisappear()
    {
        float transparentRate = 2 / (1 / 0.1f);

        Color newColor = dialogBoxImage.color;
        newColor.a -= Math.Clamp(transparentRate, 0f, 1f);
        dialogBoxImage.color = newColor;

        newColor = textBox.color;
        newColor.a -= Math.Clamp(transparentRate, 0f, 1f);
        textBox.color = newColor;

        if (newColor.a >= 1f || newColor.a <= 0f)
        {
            CancelInvoke(nameof(ImageDisappear));
            dialogBox.SetActive(false);
        }
    }
}
