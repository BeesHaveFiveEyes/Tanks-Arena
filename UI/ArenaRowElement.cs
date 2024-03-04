using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Globalization;
using TMPro;

public class ArenaRowElement : MonoBehaviour
{
    // Text objects
    public TextMeshProUGUI nameText;

    // This row's arena
    public Level level;

    public void Initialise()
    {
        UpdateVisuals();
        LevelPreview levelPreview = GetComponentInChildren<LevelPreview>();
        if (levelPreview != null)
        {            
            levelPreview.GeneratePreview(level);
        }
        if (TryGetComponent(out View view))
        {
            view.viewName = level.levelName;
        }
    }

    private void UpdateVisuals()
    {        
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

        string formattedName = textInfo.ToTitleCase(level.levelName.ToLower()); 
       
        nameText.text = formattedName;
    }

    public void InitiateDeleteArena()
    {
        MainMenuManager mainMenuManager = FindObjectOfType<MainMenuManager>();
        mainMenuManager.alert.Show("Delete this arena?", "This action cannot be undone.", DeleteArena);
    }

    private void DeleteArena()
    {
        MainMenuManager mainMenuManager = FindObjectOfType<MainMenuManager>();
        mainMenuManager.DeleteLevel(level.levelName);
    }

    public void DuplicateArena()
    {
        Preferences.passedLevel = level;
        FindObjectOfType<MainMenuManager>().LaunchLevelEditor(Context.mp_editor_duplicatingLevel);
    }

    public void EditArena()
    {
        Preferences.passedLevel = level;
        FindObjectOfType<MainMenuManager>().LaunchLevelEditor(Context.mp_editor_existingLevel);
    }
}
