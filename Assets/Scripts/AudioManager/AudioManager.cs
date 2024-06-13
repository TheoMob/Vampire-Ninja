using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    [SerializeField] private AudioMixerGroup masterOutputMixer;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;

            if (s.outputMixer != null)
                s.source.outputAudioMixerGroup = s.outputMixer;
            else
                s.source.outputAudioMixerGroup = masterOutputMixer;
        }
    }

    public void Play (string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("AudioClip " + soundName + " was not been found!");
            return;
        }

        s.source.Play();
    }

    public void Stop (string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("AudioClip " + soundName + " was not been found!");
            return;
        }

        s.source.Stop();
    }
}
