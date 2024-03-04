using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RowControl : MonoBehaviour
{
    // Text objects
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    // The preference control by this row control
    public Preferences.All preference;

    private IPreference _preference
    {
        get
        {
            return Preferences.PreferenceFor(preference);
        }
    } 
    // Can the control loop around once it has exhausted all values?
    public bool looping = true;

    private void Awake() {
        UpdateVisuals();
    }

    // Increase the current value
    public void Increment()
    {
        BooleanPreference booleanPreference = _preference as BooleanPreference;
        DiscretePreference discretePreference = _preference as DiscretePreference;
        
        if (booleanPreference != null)
        {
            booleanPreference.Toggle();
        }
        else if (discretePreference != null)
        {
            discretePreference.Increment(looping);
        }

        UpdateVisuals();
    }

    // Decrease the current value
    public void Decrement()
    {
        BooleanPreference booleanPreference = _preference as BooleanPreference;
        DiscretePreference discretePreference = _preference as DiscretePreference;
        
        if (booleanPreference != null)
        {
            booleanPreference.Toggle();
        }
        else if (discretePreference != null)
        {
            discretePreference.Decrement(looping);
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        // Update main text
        nameText.text = _preference.Name;
        valueText.text = _preference.Description;

        // Update description text
        DiscretePreference discretePreference = _preference as DiscretePreference;
        if (discretePreference != null && descriptionText != null)
        {
            if (discretePreference.explanations != null)
            {
                if (discretePreference.explanations.Length >= discretePreference.value)
                {
                    descriptionText.text = discretePreference.explanations[discretePreference.value];
                }   
            }
        }
    }
}
