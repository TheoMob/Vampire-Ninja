using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume;
    [Range(.1f, 3f)] public float pitch;

    public bool loop;

    public AudioMixerGroup outputMixer;
    
    public SoundType soundType;

    [HideInInspector] public AudioSource source;

    public enum SoundType
    {
        sound,
        music,
    }
}
