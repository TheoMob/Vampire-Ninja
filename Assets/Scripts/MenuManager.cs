using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
public class MenuManager : MonoBehaviour
{
    public AudioMixer musicAudioMixer;
    public AudioMixer soundAudioMixer;
    [HideInInspector]   public AudioManager _audioManager;
    void Start()
    {   
        _audioManager = FindObjectOfType<AudioManager>();
        _audioManager.Play("MenuMusic2", false, Vector2.zero);

        soundTextNumber = 50;
        soundText.text = soundTextNumber.ToString() + "%";
        musicTextNumber = 50;
        musicText.text = musicTextNumber.ToString() + "%";
        currentSoundVolume = -20;
        currentMusicVolume = -20;
        soundAudioMixer.SetFloat("volume", currentSoundVolume);
        musicAudioMixer.SetFloat("volume", currentMusicVolume);
    }

    public void PressPlayButton()
    {
        _audioManager.Stop("MenuMusic2");
        SceneManager.LoadScene("Forest");
    }

    private float currentSoundVolume;
    private float soundTextNumber;
    [SerializeField] private TextMeshProUGUI soundText;
    public void SoundChanger(int modifier)
    {
        _audioManager.Play("Kunai3", false, Vector2.zero);

        if (soundTextNumber + modifier > 100 || soundTextNumber + modifier < 0)
            return;

        soundTextNumber = soundTextNumber + modifier;

        soundText.text = soundTextNumber.ToString() + "%";

        modifier = modifier / 3;
        currentSoundVolume = currentSoundVolume + modifier; 
        soundAudioMixer.SetFloat("volume", currentSoundVolume);
    }

    private float currentMusicVolume;
    private float musicTextNumber;
    [SerializeField] private TextMeshProUGUI musicText;
    public void MusicChanger(int modifier)
    {
        _audioManager.Play("Kunai3", false, Vector2.zero);

        if (musicTextNumber + modifier > 100 || musicTextNumber + modifier < 0)
            return;

        musicTextNumber = musicTextNumber + modifier;
        musicText.text = musicTextNumber.ToString() + "%";

        modifier = modifier / 3;
        currentMusicVolume = currentMusicVolume + modifier; 
        musicAudioMixer.SetFloat("volume", currentMusicVolume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    [Header("Quality Toggles")]
    [SerializeField] Toggle highResToggle;
    [SerializeField] Toggle mediumResToggle;
    [SerializeField] Toggle lowResToggle;

    public void SetQuality (int qualityIndex)
    {
        _audioManager.Play("Kunai3", false, Vector2.zero);

        switch(qualityIndex)
        {
            case 2:
                if (highResToggle.isOn == false)
                    return;

                highResToggle.interactable = false;
                mediumResToggle.isOn = false;
                mediumResToggle.interactable = true;
                lowResToggle.isOn = false;
                lowResToggle.interactable = true;
            break;
            case 1:
                if (mediumResToggle.isOn == false)
                    return;

                mediumResToggle.interactable = false;
                highResToggle.isOn = false;
                highResToggle.interactable = true;
                lowResToggle.isOn = false;
                lowResToggle.interactable = true;
            break;
            case 0:
                if (lowResToggle.isOn == false)
                    return;

                lowResToggle.interactable = false;
                mediumResToggle.isOn = false;
                mediumResToggle.interactable = true;
                highResToggle.isOn = false;
                highResToggle.interactable = true;
            break;
        }

        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
