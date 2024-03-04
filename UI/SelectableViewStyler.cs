using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using JetBrains.Annotations;

public class SelectableViewStyler : MonoBehaviour
{
    public GameObject[] gameObjectsIgnoringBoldStyling;
    public GameObject[] gameObjectsIgnoringColorStyling;

    private bool initialised;

    private Image panelImage;
    private Color defaultPanelColor;

    private TextMeshProUGUI[] childTextElements;
    private Color[] childTextElementsDefaultColors;
    private FontWeight[] childTextElementsDefaultFontWeights;
        
    private Image[] childImages;
    private Color[] childImagesDefaultColors;

    public void SetHighlight(bool input)
    {
        // Create lists
        if (!initialised)
        {
            // Access back panel image and its color
            panelImage = GetComponent<Image>();
            defaultPanelColor = panelImage.color;

            // Access child images
            childImages = GetComponentsInChildren<Image>();
            childImages = Array.FindAll(childImages, image => image.gameObject != gameObject);

            // Store their default colours
            childImagesDefaultColors = new Color[childImages.Length];
            for (int i = 0; i < childImages.Length; i++)
            {
                childImagesDefaultColors[i] = childImages[i].color;                
            }

            // Access child text elements
            childTextElements = GetComponentsInChildren<TextMeshProUGUI>();

            // Store their default colours and font weights
            childTextElementsDefaultColors = new Color[childTextElements.Length];
            childTextElementsDefaultFontWeights = new FontWeight[childTextElements.Length];
            for (int i = 0; i < childTextElements.Length; i++)
            {
                childTextElementsDefaultColors[i] = childTextElements[i].color;
                childTextElementsDefaultFontWeights[i] = childTextElements[i].fontWeight;
            }

            // Close
            initialised = true;            
        }

        // Tint the panel background
        if (panelImage != null)
        {
            panelImage.color = input ? Colors.panelHighlighted : defaultPanelColor;
        }

        // Color and embolden any text
        for (int i = 0; i < childTextElements.Length; i++)
        {
            if (input)
            {
                if (!gameObjectsIgnoringColorStyling.Contains(childTextElements[i].gameObject))
                {
                    childTextElements[i].color = Colors.textHighlighted;
                }
                if (!gameObjectsIgnoringBoldStyling.Contains(childTextElements[i].gameObject))
                {
                    childTextElements[i].fontWeight = FontWeight.Bold;
                }                
            }
            else
            {
                childTextElements[i].fontWeight = childTextElementsDefaultFontWeights[i];
                childTextElements[i].color = childTextElementsDefaultColors[i];
            }
        }

        // Color any images
        for (int i = 0; i < childImages.Length; i++)
        {
            if (input)
            {
                if (!gameObjectsIgnoringColorStyling.Contains(childImages[i].gameObject))
                {
                    childImages[i].color = Colors.textHighlighted;
                }                              
            }
            else
            {                
                childImages[i].color = childImagesDefaultColors[i];
            }
        }
    }
}
