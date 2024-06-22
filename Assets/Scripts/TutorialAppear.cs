using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAppear : MonoBehaviour
{
    [SerializeField] private float timeToTutorialAppear;
    [SerializeField] private float effectSpeed;

    private bool appearing = true;
    private SpriteRenderer[] tutorialImage;
    private void Awake()
    {
        tutorialImage = new SpriteRenderer[transform.childCount];

        for(int i = 0; i < transform.childCount; i++)
        {
            tutorialImage[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();

            Color invisibleColor = tutorialImage[i].color;
            invisibleColor.a = 0;
            tutorialImage[i].color = invisibleColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        appearing = true;
        CancelInvoke(nameof(ImageAppearAndDisappear));
        InvokeRepeating(nameof(ImageAppearAndDisappear), timeToTutorialAppear, .1f);
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (!other.CompareTag("Player"))
            return;
        
        appearing = false;
        CancelInvoke(nameof(ImageAppearAndDisappear));
        InvokeRepeating(nameof(ImageAppearAndDisappear), timeToTutorialAppear, .1f); 
    }
    private void ImageAppearAndDisappear()
    {
        float transparentRate = effectSpeed / (1 / 0.1f);
        foreach (SpriteRenderer spr in tutorialImage)
        {
            Color newColor = spr.color;

            if (appearing)
                newColor.a += Math.Clamp(transparentRate, 0f, 1f);
            else
                newColor.a -= Math.Clamp(transparentRate, 0f, 1f);

            spr.color = newColor;

            if (newColor.a >= 1f || newColor.a <= 0f)
            {
                CancelInvoke(nameof(ImageAppearAndDisappear));
            }
        }
    }
}
