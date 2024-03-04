using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public bool paused = false;
    public float frameRate = 24;

    public Image target;
    public Sprite[] sprites;

    public float delay = 0;
    public bool looping = true;

    private float t;
    private int i;    

    private bool delayOver = false;

    private float frameLength
    {
        get
        {
            return 1 / frameRate;
        }
    }

    private void Awake()
    {
        UpdateImage();
    }
    
    private void Update()
    {
        if (!paused)
        {
            t += Time.deltaTime;        

            if (!delayOver)
            {
                delayOver = t > delay;
            }
            if (delayOver)
            {
                if (t > frameLength)
                {
                    t = 0;
                    i++;

                    UpdateImage();
                }                    
            }            
        }
    }

    private void UpdateImage()
    {
        if (looping)
        {
            target.sprite = sprites[i % sprites.Length];
        }        
        else
        {
            target.sprite = sprites[Math.Min(i, sprites.Length - 1)];
        }
    }
}
