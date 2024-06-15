using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IPlayerState;
using System;
using Cinemachine;
using NaughtyAttributes;

public class Dialog : MonoBehaviour
{
    [SerializeField] private string text = "Não temos muito tempo coff... coff... Vá  antes que seja tarde...";
    [SerializeField] private bool lockCamera;
    [ShowIf(nameof(lockCamera))] [SerializeField] private CinemachineVirtualCamera closeUpCamera;
    private GameObject dialogBox;
    private SpriteRenderer dialogBoxImage;
    private TextMeshPro textBox;
    [SerializeField] private float textDelay;
    [SerializeField] private float dialogDuration;

    private PlayerStateController _stateController;
    private AudioManager _audioManager;
    public bool dialogAlreadyPlayed = false;
    public bool skipDialog = false;

    private Color initialBoxColor;
    private Color initialTextColor;
    private void Start()
    {
        _stateController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();
        _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();

        dialogBox = transform.GetChild(0).gameObject;
        dialogBoxImage = dialogBox.GetComponent<SpriteRenderer>();

        textBox = dialogBox.transform.GetChild(0).GetComponent<TextMeshPro>();
        
        initialBoxColor = dialogBoxImage.color;
        initialTextColor = textBox.color;

        dialogBox.SetActive(false);
    }

    private bool odd = true;
    [SerializeField] private string voice1 = "Voice1";
    [SerializeField] private string voice2 = "Voice2";

    IEnumerator TypeWriterEffect()
    {
        for (int i = 0; i < text.Length; i++)
        {
            odd = !odd;

            if (odd)
                _audioManager.Play(voice1, false, Vector2.zero);

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

        if (lockCamera)
            closeUpCamera.Priority = 09;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (dialogAlreadyPlayed || !other.CompareTag("Player"))
            return;

        CallDialog();
    }

    public void CallDialog()
    {
        StartCoroutine(StartDialog());
    }

    IEnumerator StartDialog()
    {
        if (lockCamera)
            closeUpCamera.Priority = 11;

        _stateController.cantMove = true;
        dialogAlreadyPlayed = true;

        yield return new WaitForSeconds(1);

        if (skipDialog)
        {
            _stateController.cantMove = false;
            yield break;
        }

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
            textBox.color = initialTextColor;
            dialogBoxImage.color = initialBoxColor;
        }
    }
}
