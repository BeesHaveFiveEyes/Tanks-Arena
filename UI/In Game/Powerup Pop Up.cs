using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PowerupPopUp : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpSprite {
        [SerializeField] public Sprite sprite;
        public PowerUp powerUpType;
    }

    // The transform above which to show the popup
    public Transform target;

    // The power up type
    public PowerUp powerUpType;

    // The time for which the popup remains visible
    public float duration = 4;

    // The time the popup takes to appear and dissappear
    public float transitionDuration = 0.2f;

    // The vertical pop up offset in canvas space
    public float verticalOffset = 0;

    // The pop up image
    public Image image;

    // The image corresponding to each powerup
    public PowerUpSprite[] powerUpSprites;

    // A timer for the lifetime of the popup
    private float timer = 999;

    public void Start()
    {  
        // Check a target has been provided
        if (target == null)
        {
            Debug.LogError("Powerup pop up not supplied with a target");
        }

        // Load in the appropriate image        
        image.sprite = Array.Find(powerUpSprites, item => item.powerUpType == powerUpType).sprite;

        // Scale in
        transform.localScale = Vector3.one * 0.01f;
        LeanTween.scale(gameObject, Vector3.one, transitionDuration)
        .setEase(LeanTweenType.easeSpring);   

        // Initialise timer
        timer = duration;
    }

    private void Update() {
        timer -= Time.deltaTime;
        FollowTank();
        if (timer <= 0)
        {    
            // Scale out
            LeanTween.scale(gameObject, Vector3.zero, transitionDuration)
            .setEase(LeanTweenType.clamp)
            .setOnComplete(() =>
            {
                Destroy(gameObject);
            });      
        }
    }

    // Remain above the supplied target transform
    private void FollowTank() {
        if (target == null)
        {
            timer = -1;
        }
        else
        {
            // Calculate screen position        
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(target.position);
            
            // Find the canvas
            RectTransform canvasRect = FindObjectOfType<Canvas>().GetComponent<RectTransform>();

            // Convert screen position to Canvas space
            Vector2 initialCanvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out initialCanvasPosition);
            
            // Calculate vertical offset
            float offset = verticalOffset + Mathf.Sin(6 * Time.timeSinceLevelLoad);

            // Apply the vertical offset
            Vector2 finalCanvasPosition = new Vector2(initialCanvasPosition.x, initialCanvasPosition.y + offset);
            
            // Apply the final position
            transform.localPosition = finalCanvasPosition;    
        }
    }
}
