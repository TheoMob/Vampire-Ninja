using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapAudioHandler : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    }

    public void playSound(string soundName)
    {
        audioManager.Play(soundName);
    }
}
