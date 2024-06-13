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
        audioManager.Play("Footstep" + footStepIndex);
    }

    public void playJumpSound()
    {
        audioManager.Play("Jump");
    }

    public void playWhooshSound()
    {
        audioManager.Play("Whoosh");
    }
}
