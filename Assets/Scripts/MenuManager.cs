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

    public AudioManager _audioManager;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullScreenToggle;

    private Resolution[] resolutions;

    private Vector2[] availableResolutions;
    void Start()
    {   
        // SetAvailableResolutions();
        // InitiateResolutionDropdown();
        _audioManager = FindObjectOfType<AudioManager>();

        soundAudioMixer.SetFloat("volume", 0);
        musicAudioMixer.SetFloat("volume", 0);

        fullScreenToggle.isOn = Screen.fullScreen;
    }

      #region resolutions dropdown
    // private void SetAvailableResolutions()
    // {
    //     availableResolutions = new Vector2[5];
    //     availableResolutions[0] = new Vector2(1920, 1080);
    //     availableResolutions[1] = new Vector2(1366, 768);
    //     availableResolutions[2] = new Vector2(1280, 1024);
    //     availableResolutions[3] = new Vector2(1024, 768);
    //     availableResolutions[4] = new Vector2(800, 600);
    // }
    // private void InitiateResolutionDropdown()
    // {
    //     resolutions = Screen.resolutions;
    //     resolutionDropdown.ClearOptions();

    //     List<string> options = new List<string>();

    //     int currentResolutionIndex = 0;
    //     for(int i = 0; i < resolutions.Length; i++)
    //     {
    //         Resolution resolutionSize = resolutions[i];

    //         bool resolutionFound = false;
    //         for(int j = 0; j < availableResolutions.Length; j++) // this is so only the selected resolutions appear, if this is removed Unity will list EVERY possible resolution
    //         {
    //             Vector2 testedResolution = availableResolutions[j];
    //             if (resolutionSize.width == testedResolution.x && resolutionSize.height == testedResolution.y)
    //             {
    //                 resolutionFound = true;
    //                 break;
    //             }
    //         }

    //         if (resolutionFound)
    //         {
    //             string optionText = resolutionSize.width + " x " + resolutionSize.height;
    //             options.Add(optionText);

    //             if (resolutionSize.width == Screen.currentResolution.width && resolutionSize.height == Screen.currentResolution.height)
    //                 currentResolutionIndex = i;
    //         }
    //     }

    //     resolutionDropdown.AddOptions(options);
    //     resolutionDropdown.value = currentResolutionIndex;
    //     resolutionDropdown.RefreshShownValue();
    // }
     #endregion
    public void PressPlayButton()
    {
        _audioManager.Stop("MenuMusic2");
        _audioManager.Play("MusicaPlaceholder");
        SceneManager.LoadScene("Forest");
    }

    public void PressExitButton()
    {
        Application.Quit();
    }

    public void SoundSlider(float volume)
    {
        soundAudioMixer.SetFloat("volume", volume);
    }

    public void MusicSlider(float volume)
    {
        musicAudioMixer.SetFloat("volume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    [Header("Quality Toggles")]
    [SerializeField] Toggle highResToggle;
    [SerializeField] Toggle mediumResToggle;
    [SerializeField] Toggle lowResToggle;


    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality (int qualityIndex)
    {
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
