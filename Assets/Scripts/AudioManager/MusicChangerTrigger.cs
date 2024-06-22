using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChangerTrigger : MonoBehaviour
{
    private bool alreadyChanged = false;
    [SerializeField] private int nextTrackIndex;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag("Player") || alreadyChanged)
            return;
        
        alreadyChanged = true;
        AudioManager _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
        _audioManager.currentTrackIndex = nextTrackIndex;
    }
}
