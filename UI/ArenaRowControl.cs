using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Globalization;
using TMPro;

public class ArenaRowControl : MonoBehaviour
{
    // Text objects
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI nameText;

    // The preference control by this row control
    public Level level;

    public void Initialise() {
        UpdateVisuals();
        LevelPreview levelPreview = GetComponentInChildren<LevelPreview>();
        if (levelPreview != null)
        {
            levelPreview.GeneratePreview(level);
        }
    }

    // Increase the current value
    public void Increment()
    {
        Toggle();               
    }

    // Decrease the current value
    public void Decrement()
    {
        Toggle();        
    }

    public void Toggle()
    {
        Preferences.ToggleArenaSelection(level.levelName);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {        
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        string formattedName = textInfo.ToTitleCase(level.levelName.ToLower()); 
       
        nameText.text = formattedName;
        valueText.text = Preferences.ArenaSelection(level.levelName) ? "On" : "Off";
    }
}
