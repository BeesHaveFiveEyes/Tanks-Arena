using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridMarker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float defaultAlpha = 0.5f;
    public float highlightAlpha = 1;

    public Sprite defaultSprite;
    public Sprite highlightSprite;

    public void SetHighlight(bool input)
    {
        // Apply the highlight alpha
        Color color = spriteRenderer.color;
        color.a = input ? highlightAlpha : defaultAlpha;
        spriteRenderer.color = color;

        // Apply the highlight sprite
        spriteRenderer.sprite = input ? highlightSprite : defaultSprite;
    }
}
