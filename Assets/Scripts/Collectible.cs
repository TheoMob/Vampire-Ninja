using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Animator animator;
    private const string COLLECT_ANIMATION = "CollectAnimation";

    private void Awake()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag("Player"))
            return;
        
        AudioManager _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
        _audioManager.Play("Smoke3", false, Vector2.zero);

        animator.Play(COLLECT_ANIMATION);
        //float collectAnimationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(DestroyCollectable), 0.9f);
    }

    private void DestroyCollectable()
    {
        Destroy(gameObject);
    }
}
