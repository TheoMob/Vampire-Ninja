using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundHandler : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    }

    public void playFootStepSound(int footStepIndex)
    {
        audioManager.Play("Footstep" + footStepIndex, false, Vector2.zero);
    }

    [SerializeField] private string jumpSound = "Jump1";
    public void playJumpSound()
    {
        audioManager.Play(jumpSound, false, Vector2.zero);
    }

    public void playWhooshSound()
    {
        audioManager.Play("Whoosh", false, Vector2.zero);
    }
}
