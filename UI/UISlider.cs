using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlider : MonoBehaviour
{
    public enum SlideType
    {
        top, bottom, left, right
    }

    public LeanTweenType showEasing = LeanTweenType.easeOutExpo;
    public LeanTweenType dismissEasing = LeanTweenType.easeInSine;

    [SerializeField] private SlideType slideType;

    public float transitionTime = 0.5f;

    private Vector2 onPivot;
    private Vector2 offPivot;
    private Vector2 onPosition;
    private Vector2 offPosition;
    private Vector2 lastPosition;
    private Vector2 lastPivot;

    private RectTransform item;
    private bool initialised;

    private void Initialise()
    {
        SetOnPosition();
        SetOffPosition();
    }

    private void SetOnPosition()
    {
        if (TryGetComponent(out RectTransform itemFound))
        {
            item = itemFound;
        }
        else
        {
            Debug.LogError("No Rect Transform Found.");
        }

        onPivot = item.pivot;
        onPosition = item.anchoredPosition;
    }

    private void SetOffPosition()
    {
        float itemWidth = item.rect.width * item.localScale.x;
        float itemHeight = item.rect.height * item.localScale.y;

        switch (slideType)
        {
            case SlideType.top:
                offPivot = new Vector2(0.5f, 1);
                offPosition = new Vector2(onPosition.x, itemHeight);
                break;
            case SlideType.bottom:
                offPivot = new Vector2(0.5f, 0);
                offPosition = new Vector2(onPosition.x, -itemHeight);
                break;
            case SlideType.left:
                offPivot = new Vector2(0, 0.5f);
                offPosition = new Vector2(-itemWidth, onPosition.y);
                break;
            case SlideType.right:
                offPivot = new Vector2(1, 0.5f);
                offPosition = new Vector2(itemWidth, onPosition.y);
                break;
        }

        initialised = true;
    }

    public void SetSlideType(SlideType _slideType)
    {
        LeanTween.cancel(gameObject);
        slideType = _slideType;
        SetOffPosition();
    }

    private void GuardInitialisation()
    {
        if (!initialised)
        {
            Initialise();
        }
    }

    private void Start()
    {
        GuardInitialisation();
    }

    public void ChangeSlideType(SlideType newType)
    {
        slideType = newType;
        Initialise();
    }

    public void Show(float duration = -1)
    {
        if (duration < 0)
        {
            duration = transitionTime;
        }

        GuardInitialisation();

        Vector2 startingPosition;
        Vector2 startingPivot;

        if (!gameObject.activeInHierarchy)
        {           
            startingPosition = offPosition;
            startingPivot = offPivot; 
        }
        else
        {
            startingPosition = item.anchoredPosition;
            startingPivot = item.pivot;
        }

        gameObject.SetActive(true);
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, 0, 1, duration).setOnUpdate((float t) =>
        {
            item.pivot = startingPivot + t * (onPivot - startingPivot);
            item.anchoredPosition = startingPosition + t * (onPosition - startingPosition);
        })
        .setEase(showEasing);
    }

    public void Dismiss(float duration = -1)
    {        
        if (duration < 0)
        {
            duration = transitionTime;
        }

        GuardInitialisation();

        Vector2 startingPosition = item.anchoredPosition;
        Vector2 startingPivot = item.pivot;

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, 1, 0, duration).setOnUpdate((float t) =>
        {
            item.pivot = offPivot + t * (startingPivot - offPivot);
            item.anchoredPosition = offPosition + t * (startingPosition - offPosition);
        })
        .setEase(dismissEasing)
        .setOnComplete(() => gameObject.SetActive(false));
    }
}
