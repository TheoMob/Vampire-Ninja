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
    [SerializeField] private AudioMixerGroup musicOutputMixer;
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

    [SerializeField] private float distanceToSoundToWork = 285f;
    public void Play (string soundName, bool checkForPlayerPosition, Vector2 soundPosition)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("AudioClip " + soundName + " was not been found!");
            return;
        }

        if (!checkForPlayerPosition)
            s.source.Play();
        else
        {
            Vector2 playerPos = GameObject.FindWithTag("Player").transform.position;
            if (Vector2.Distance(playerPos, soundPosition) <= distanceToSoundToWork)
                s.source.Play();
        }
    }

    public void PlaySimple(string soundName)
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


    public int currentTrackIndex = 1;

    public void StartMusicHandler()
    {
        StartCoroutine(MusicTimerHandler());
    }

    IEnumerator MusicTimerHandler()
    {
        switch(currentTrackIndex)
        {
            case 1:
                Debug.Log("Now Playing Finalmusic1");
                PlaySimple("FinalMusic1");
                yield return new WaitForSeconds(25);
            break;
            case 2:
                Debug.Log("Now Playing Finalmusic2");
                PlaySimple("FinalMusic2");
                yield return new WaitForSeconds(25);
            break;
            case 3:
                Debug.Log("Now Playing Finalmusic3");
                PlaySimple("FinalMusic3");
                yield return new WaitForSeconds(25);
            break;
            case 4:
                Debug.Log("Now Playing Finalmusic4");
                PlaySimple("FinalMusic4");
                yield return new WaitForSeconds(27);
            break;
            case 5:
                Debug.Log("Now Playing Finalmusic5");
                PlaySimple("FinalMusic5");
                yield return new WaitForSeconds(110);
            break;
        }

        StartCoroutine(MusicTimerHandler());
    }
}
