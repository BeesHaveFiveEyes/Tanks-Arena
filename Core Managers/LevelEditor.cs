using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    public enum LevelEditorTool
    {
        none,
        eraser,
        drawWall,
        crate,
        barrel,
        powerUp,
        player,    
        yellow,
        green,
        orange,
        rocket,
        navy,
        black,
        rotate,
        pan,
        zoom
    }        

    public LevelEditorTool currentTool = LevelEditorTool.drawWall;    

    // UI Components
    public UIFader fader;
    public Alert alert;
    public TextMeshProUGUI levelNameInputField;
    public List<GameObject> singlePlayerOnlyTools;
    public List<GameObject> multiplayerOnlyTools;

    // Initialisation properties
    private static bool initialised = false;
    private static LevelEditor staticCurrentLevelEditor;

    // The current GameManager instance
    public static LevelEditor current {
        get {
            if (!initialised) {
                staticCurrentLevelEditor =  FindObjectOfType<LevelEditor>();
                initialised = true;
            }
            return staticCurrentLevelEditor;
        }        
    }

    private void Start()
    {
        // Show the cursor
        Cursor.visible = true;

        // Show / hide the appropriate tools
        foreach (GameObject tool in multiplayerOnlyTools)
        {
            tool.SetActive(Preferences.Multiplayer);
        }
        foreach (GameObject tool in singlePlayerOnlyTools)
        {
            tool.SetActive(!Preferences.Multiplayer);
        }
        alert.inputController.rootView.Refresh();

        // Create the arena
        switch (Preferences.context)
        {
            case Context.mp_editor_duplicatingLevel:
            case Context.mp_editor_existingLevel:                    
            case Context.sp_editor_duplicatingLevel:
            case Context.sp_editor_existingLevel:                
                Arena.CreateArena(Preferences.passedLevel);       
                break;
            case Context.mp_editor_newLevel:
            case Context.sp_editor_newLevel:
            default:
                Arena.CreateEmptyArena();
                break;            
        }


        // Fade in from black 
        fader.gameObject.SetActive(true);
        fader.Dismiss();
    }

    private void ResetLevel()
    {
        Arena.CreateEmptyArena();
    }

    // When a block is clicked:
    public void OnClick(Block block)
    {
        Debug.Log(currentTool);
        if (!MouseBlocker.blocking)
        {
            switch (currentTool)
            {
                case LevelEditorTool.eraser:
                    if (block.tile.closed || block.tile.surfaceObject != Tile.SurfaceObject.none) AudioManager.Play("Eraser");
                    block.tile.surfaceObject = Tile.SurfaceObject.none;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.drawWall:
                    if (!block.tile.closed) AudioManager.Play("Wall Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.none;
                    block.tile.closed = true;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.crate:
                    if (block.tile.surfaceObject != Tile.SurfaceObject.crate) AudioManager.Play("Crate Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.crate;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.barrel:
                    if (block.tile.surfaceObject != Tile.SurfaceObject.redBarrel) AudioManager.Play("Barrel Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.redBarrel;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.powerUp:
                    AudioManager.Play("Powerup Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.powerUp;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.player:                    
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.playerParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;               
                case LevelEditorTool.yellow:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.yellowParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.green:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.greenParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.orange:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.orangeParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.rocket:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.rocketParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure();
                    break;
                case LevelEditorTool.navy:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.navyParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
                case LevelEditorTool.black:
                    AudioManager.Play("Tank Placed");
                    block.tile.surfaceObject = Tile.SurfaceObject.blackParts;
                    block.tile.closed = false;
                    block.tile.wired = false;
                    block.Reconfigure(true);
                    break;
            }
        }
    }

    // When the mouse is dragged over a block whilst held down:
    public void OnDrag(Block block)
    {
        if (!MouseBlocker.blocking)
        {
            if (currentTool == LevelEditorTool.eraser
            || currentTool == LevelEditorTool.drawWall
            || currentTool == LevelEditorTool.crate)
            {
                OnClick(block);
            }
        }
    }

    // Save the new arena to storage for use in gameplay
    private void SaveArenaToStorage(string levelName)
    {
        if (Preferences.Multiplayer)
        {
            LevelManager.SaveCustomMultiplayerLevelToFile(Arena.currentArena.level, levelName);
            Debug.Log("Saved multiplayer level '" + levelName + "'.");
        }
        else
        {
            LevelManager.SaveSinglePlayerLevelToFile(Arena.currentArena.level, levelName);
            Debug.Log("Saved single player evel '" + levelName + "'.");
        }   

        AudioManager.Play("Menu Success");
        TransitionToMainMenu();
    }

    private void SaveNewArenaToStorage()
    {
        string input = levelNameInputField.text;
        SaveArenaToStorage(input);
    }

    private void UpdateArenaToStorage()
    {
        string levelName = Preferences.passedLevel.levelName;
        SaveArenaToStorage(levelName);
    }

    private void TransitionToMainMenu()
    {
        void Transition()
        {
            SceneManager.LoadScene("MainMenu");
        }        
        fader.Show(0.5f, Transition);
        AudioManager.FadeOutAll(0.5f);
    }

    // private void SaveLevel(multiplayer)

    // --------------------
    // UnityEvent Functions
    // --------------------

    public void SelectDrawWall()
    {
        currentTool = LevelEditorTool.drawWall;
    }

    public void SelectCrate()
    {
        currentTool = LevelEditorTool.crate;
    }

    public void SelectBarrel()
    {
        currentTool = LevelEditorTool.barrel;
    }

    public void SelectPowerup()
    {
        currentTool = LevelEditorTool.powerUp;
    }

    public void SelectPlayerTank()
    {
        currentTool = LevelEditorTool.player;
    }

    public void SelectYellowTank()
    {
        currentTool = LevelEditorTool.yellow;
    }

    public void SelectGreenTank()
    {
        currentTool = LevelEditorTool.green;
    }

    public void SelectOrangeTank()
    {
        currentTool = LevelEditorTool.orange;
    }

    public void SelectNavyTank()
    {
        currentTool = LevelEditorTool.navy;
    }

    public void SelectRocketTank()
    {
        currentTool = LevelEditorTool.rocket;
    }

    public void SelectBlackTank()
    {
        currentTool = LevelEditorTool.black;
    }

    public void SelectEraser()
    {
        currentTool = LevelEditorTool.eraser;
    }

    public void DeselectTool()
    {
        currentTool = LevelEditorTool.none;
    }

    public void InitiateResetLevel()
    {
        alert.Show("Reset this arena?", "This action cannot be undone.", ResetLevel);
    }

    public void InitiateQuitWithoutSaving()
    {
        alert.Show("Quit Without Saving?", "Any changes made will be lost.", TransitionToMainMenu);
    }

    public void InitiateSaveLevel()
    {
        // Check how many player tanks have been placed
        int playersPlaced = 0;
        int opponentsPlaced = 0;
        foreach (List<Tile> row in Arena.currentArena.level.tileMatrix)
        {
            foreach (Tile tile in row)
            {
                if (tile.surfaceObject == Tile.SurfaceObject.playerParts)
                {
                    playersPlaced++;
                }  
                else if (
                    tile.surfaceObject == Tile.SurfaceObject.yellowParts
                    || tile.surfaceObject == Tile.SurfaceObject.greenParts
                    || tile.surfaceObject == Tile.SurfaceObject.orangeParts
                    || tile.surfaceObject == Tile.SurfaceObject.navyParts
                    || tile.surfaceObject == Tile.SurfaceObject.rocketParts
                    || tile.surfaceObject == Tile.SurfaceObject.blackParts
                )
                {
                    opponentsPlaced++;    
                }              
            }
        }
        
        if (Preferences.Multiplayer)
        {
            if (opponentsPlaced > 0)
            {
                alert.Show("AI Tanks Detected", "To save your arena, please remove all AI tank spawn points.");
            }
            
            else if (playersPlaced < 2)
            {
                alert.Show("Missing Tanks", "Please place two player tank spawn points before saving your arena.");
            }

            else if (playersPlaced > 2)
            {
                alert.Show("Too Many Tanks", "Ensure there are exactly two player tank spawn points before saving your arena.");
            }

            else 
            {       
                switch(Preferences.context)   
                {
                    case Context.mp_editor_existingLevel:
                        alert.Show("Update this arena?", "All changes you have made to this arena will be saved.", UpdateArenaToStorage);
                        break;
                    case Context.mp_editor_duplicatingLevel:
                        alert.ShowInputAlert("Save as new arena?", "This new arena will be available for use in multiplayer games.", ValidateMultiplayerName, SaveNewArenaToStorage);                        
                        break;
                    default:
                        alert.ShowInputAlert("Save this arena?", "Your new arena will be available for use in multiplayer games.", ValidateMultiplayerName, SaveNewArenaToStorage);                        
                        break;
                }                      
            }
        }
        else
        {
            if (playersPlaced < 1)
            {
                alert.Show("No Player Detected", "To save your arena, please add a spawn point for the player tank");
            }

            else if (playersPlaced > 1)
            {
                alert.Show("Multiple Player Tanks", "To save your arena, please ensure it contains exactly one player tank spawn point");
            }

            else if (opponentsPlaced == 0)
            {
                alert.Show("No Enemy Tanks", "To save your arena, please ensure it contains at least one enemy tank spawn point.");
            }

            else 
            {
                switch(Preferences.context)   
                {
                    case Context.sp_editor_existingLevel:
                        alert.Show("Update this arena?", "All changes you have made to this arena will be saved.", UpdateArenaToStorage);
                        break;
                    case Context.sp_editor_duplicatingLevel:
                        alert.ShowInputAlert("Save as new arena?", "Your new arena will be added to the single player campaign.", ValidateSinglePlayerName, SaveNewArenaToStorage);
                        break;
                    default:
                        alert.ShowInputAlert("Save this arena?", "Your new arena will be added to the single player campaign.", ValidateSinglePlayerName, SaveNewArenaToStorage);
                        break;
                }                
            }
        }
        
    }

    public string ValidateSinglePlayerName(string input)
    {
        Debug.Log("Validating Input: " + input);

        if (input == "")
        {
            return "Arena names cannot be empty.";
        }
        if (input.Length > 20)
        {
            return "Please use a shorter name.";
        }
        if (LevelManager.SinglePlayerLevelNames().Contains(input))
        {
            return "A single player arena with this name already exists.";
        }
        
        return "";
    }

    public string ValidateMultiplayerName(string input)
    {
        if (input == "")
        {
            return "Arena names cannot be empty.";
        }
        if (input.Length > 20)
        {
            return "Please use a shorter name.";
        }
        if (LevelManager.AllMultiplayerLevelNames().Contains(input))
        {
            return "A multiplayer arena with this name already exists.";
        }
        
        return "";
    }
//     // Show save pop-up menu:
//     public void ShowSavePopUp()
//     {        
//         int playersPlaced = 0;

//         foreach (List<Tile> row in Arena.currentArena.level.tileMatrix)
//         {
//             foreach (Tile tile in row)
//             {
//                 if (tile.surfaceObject == Tile.SurfaceObject.playerParts)
//                 {
//                     playersPlaced++;
//                 }                
//             }
//         }

//         if (!(GameManager.Multiplayer && playersPlaced == 1)
//         || (GameManager.Multiplayer && playersPlaced == 2))
//         {
//             bool q = true;

//             if (PlayerPrefs.HasKey("new"))
//             {
//                 if (PlayerPrefs.GetInt("new") == 0)
//                 {
//                     q = false;
//                 }
//             }

//             // Creating a new level

//             if (GameManager.context == GameManager.Context.mp_editor_newLevel || GameManager.context == GameManager.Context.sp_editor_newLevel)
//             {
//                 saveConfirmation.Show(0.2f);
//                 OnType();
//                 fader.Show(0.2f);
//             }

//             // Editing a pre-existing level
//             else
//             {
//                 if (PlayerPrefs.HasKey("loading"))
//                 {
//                     string name = PlayerPrefs.GetString("loading");
//                     SaveLevel(name, true);
//                     Debug.Log("Updated level '" + name + "'");
//                 }
//                 else
//                 {
//                     Debug.LogError("No name given in PlayerPrefs for level to edit");
//                 }

//                 ToMainMenu();
//             }
//         }
//         else
//         {
//             string warning =
//                 GameManager.Multiplayer ?
//                 "Please insert exactly two tanks before saving the level."
//                 : "Please insert the player tank before saving the level.";

//             ShowWarning(warning);
//         }
//     }

//     // Save the level:
//     private void SaveLevel(string name, bool multiplayer)
//     {
//         if (multiplayer)
//         {
//             LevelManager.SaveMultiplayerLevelToFile(Arena.currentArena.level, name);
//             Debug.Log("Saved multiplayer level '" + name + "'.");
//         }
//         else
//         {
//             LevelManager.SaveSinglePlayerLevelToFile(Arena.currentArena.level, name);
//             Debug.Log("Saved single player evel '" + name + "'.");
//         }                
//     }

//     // Show a warning with the given text:
//     public void ShowWarning(string text)
//     {        
//         warningText.text = text;
//         warning.Show(0.2f);
//         fader.Show(0.2f);        
//     } 

//     public void OnType()
//     {
//         string proposedLevelName = levelNameInputField.text;
//         proposedLevelName = proposedLevelName.ToUpper();
//         levelNameInputField.text = proposedLevelName;

//         bool disable = proposedLevelName == "";

//         List<string> levelNames = GameManager.Multiplayer ? LevelManager.MultiplayerLevelNames() : LevelManager.SinglePlayerLevelNames();

//         if (levelNames.Contains(proposedLevelName))
//         {
//             disable = true;
//         }

//         saveButton.SetDisabled(disable);
//     }

//     // -------------------
//     // For Buttons
//     // -------------------

//     // Create new level
//     public void CreateNewLevelButton()
//     {
//         StartCoroutine(DoIt());

//         IEnumerator DoIt()
//         {
//             yield return new WaitForSeconds(0.35f);
//             Arena.CreateEmptyArena(29, 29);
//         }
//     }

//     // Save single player level
//     public void SaveSinglePlayerLevelButton()
//     {
//         string name = inputText.text;
//         SaveLevel(name, false);
//         ToMainMenu();
//     }

//     // Save multiplayer level
//     public void SaveMultiPlayerLevelButton()
//     {        
//         string name = inputText.text;
//         SaveLevel(name, true);
//         ToMainMenu();
//     }

//     public void PlaySlide()
//     {
//         AudioManager.Play("Slide");
//     }
}
