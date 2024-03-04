using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Preferences : MonoBehaviour
{
    public static Context context = Context.singlePlayer;

    public static bool Multiplayer
    {
        get
        {
            return context == Context.multiplayer
            || context == Context.mp_editor_newLevel
            || context == Context.mp_editor_existingLevel
            || context == Context.mp_editor_duplicatingLevel;
        }
    }

    public static Level passedLevel;
    public static string lastMainMenuRootView;
    public static string lastMainMenuPreviousView;
    
    public static bool Editing
    {
        get {
            return context == Context.mp_editor_newLevel
            || context == Context.mp_editor_existingLevel
            || context == Context.mp_editor_duplicatingLevel
            || context == Context.sp_editor_newLevel
            || context == Context.sp_editor_existingLevel
            || context == Context.sp_editor_duplicatingLevel;
        }
    }

    public enum All
    {
        // Single Player
        difficulty,

        // Multiplayer
        duration,

        // Audio
        soundEffects,
        music,

        // Power ups
        rockets,
        homingRockets,
        shields,
        rapidFire,

        // Camera
        singlePlayerCameraType,
        multiplayerCameraType,

        // Modifiers
        darkMode,
        dizzyMode

    }

    // Example
    private static BooleanPreference examplePreference = new BooleanPreference("p_example", "Example Preference");

    // Single player
    public static DiscretePreference difficulty = new DiscretePreference("p_difficulty", "Difficulty", new string[]{"Casual", "Easy", "Normal", "Hard"}, 2,
        new string[] {
            "Play without fear of death with unlimited lives. Recommended for casual players and younger gamers.",
            "Start with 3 lives, and regain one life every 3 rounds. Recommended for new players.",
            "Play with 3 lives for the entire game. The intended way to experience the game.",
            "Play with only 1 life for the entire game. Not recommended for the faint hearted."
        }
    );

    public static DiscretePreference singlePlayerCameraType = new DiscretePreference("p_sp_camera", "Camera Mode", new string[]{"Static", "Default", "Tight"}, 1,
        new string[] {
            "A fixed, motionless camera.",
            "A balanced camera with a small amount of movement.",
            "An immersive camera that closely follows the player."            
        }
    );

    // Multiplayer
    public static DiscretePreference duration = new DiscretePreference("p_duration", "Duration", new string[]{"1 Round", "3 Rounds", "5 Rounds", "10 Rounds", "20 Rounds", "Unlimited"}, 3);
    public static DiscretePreference multiplayerCameraType = new DiscretePreference("p_mp_camera", "Camera Mode", new string[]{"Static", "Default"}, 1);

    // Audio
    public static BooleanPreference soundEffects = new BooleanPreference("p_sound", "Sound Effects");
    public static BooleanPreference music = new BooleanPreference("p_music", "Music");

    // Power ups
    public static BooleanPreference rockets = new BooleanPreference("p_rockets", "Rockets");
    public static BooleanPreference homingRockets = new BooleanPreference("p_homing_rockets", "Homing Rockets");
    public static BooleanPreference shields = new BooleanPreference("p_shields", "Shields");
    public static BooleanPreference rapidFire = new BooleanPreference("p_rapid_fire", "Rapid Fire");

    // Challenge odifiers
    public static BooleanPreference darkMode = new BooleanPreference("p_dark_mode", "Dark Mode", "On", "Off", false);
    public static BooleanPreference dizzyMode = new BooleanPreference("p_dizzy_mode", "Dizzy Mode", "On", "Off", false);    
    // Dizzy mode!
    // Faster bullets?

    // ----------
    // Statistics
    // ----------

    public enum Statistic
    {
        highestLevelReached,
        difficulty,
        tanksDefeated,
        totalPlayerDeaths,
        finalMultiplayerScore,
        gameDuration,
        multiplayerOutcome
    }

    public static float gameDuration;
    public static int totalPlayerDeaths;
    public static int tanksDefeated;
    public static string highestLevelReached;
    public static bool singlePlayerGameCompleted;

    // Reset the statistics for a new game
    public static void ResetStatistics()
    {
        gameDuration = 0;
        totalPlayerDeaths = 0;
        tanksDefeated = 0;
        highestLevelReached = "None";
        singlePlayerGameCompleted = false;
    }

    // Access a statistic
    public static string StatisticValue(Statistic statistic)
    {
        switch (statistic)
        {
            case Statistic.highestLevelReached:
                return highestLevelReached;
            case Statistic.difficulty:
                return difficulty.DescriptionFor(difficulty.value);
            case Statistic.tanksDefeated:
                return tanksDefeated.ToString();
            case Statistic.totalPlayerDeaths:
                return totalPlayerDeaths.ToString();
            case Statistic.finalMultiplayerScore:
                return GameManager.player1Score + " : " + GameManager.player2Score;
            case Statistic.gameDuration:         
                return gameDuration.ToString();  
            case Statistic.multiplayerOutcome:
                if (GameManager.player1Score > GameManager.player2Score)
                {
                    return "Player 1 Wins!";
                }
                else if (GameManager.player2Score > GameManager.player1Score)
                {
                    return "Player 2 Wins!";
                }
                else
                {
                    return "It's a draw!";
                }
            default:
                return "Unknown";
        }
    }

    // ---------------
    // Arena selection
    // ---------------
    
    private static Dictionary<string, bool> arenaSelections = new Dictionary<string, bool>();

    public static bool ArenaSelection(string arenaName)
    {
        if (arenaSelections.TryGetValue(arenaName, out bool selection))
        {
            return selection;
        }
        else
        {
            arenaSelections.Add(arenaName, true);
            return true;
        }
    }

    public static void ToggleArenaSelection(string arenaName)
    {
        if (arenaSelections.TryGetValue(arenaName, out bool selection))
        {
            arenaSelections[arenaName] = !selection;
        }
        else
        {
            arenaSelections.Add(arenaName, false);
        }
    }

    // ----------------
    // Helper functions
    // ----------------

    public static IPreference PreferenceFor(All preference)
    {
        switch (preference)
        {
            case All.difficulty:
                return difficulty;
            case All.duration:
                return duration;
            case All.soundEffects:
                return soundEffects;
            case All.music:
                return music;
            case All.rockets:
                return rockets;
            case All.homingRockets:
                return homingRockets;
            case All.shields:
                return shields;
            case All.rapidFire:
                return rapidFire;   
            case All.singlePlayerCameraType:
                return singlePlayerCameraType;
            case All.multiplayerCameraType:
                return multiplayerCameraType;
            case All.darkMode:
                return darkMode;
            case All.dizzyMode:
                return dizzyMode;
            default:
                return examplePreference;

        }
    }

    public static int MultiplayerDuration
    {
        get
        {
            switch (duration.value)
            {
                case 0:
                    return 1;
                case 1:
                    return 3;
                case 2:
                    return 5;
                case 3:
                    return 10;
                case 4:
                    return 20;                
                default:
                    return int.MaxValue;
            }
        }
    }

    public static CameraController.State cameraState
    {
        get
        {
            if (Multiplayer)
            {
                switch (multiplayerCameraType.value)
                {
                    case 0:
                        return CameraController.State.stationary;
                    case 1:
                    default:
                        return CameraController.State.standard;                    
                }
            }
            else
            {
                if (dizzyMode.value)
                {
                    return CameraController.State.dizzy;
                }
                else 
                {
                    switch (singlePlayerCameraType.value)
                    {
                        case 0:
                            return CameraController.State.stationary;
                        case 1:
                            return CameraController.State.standard;                    
                        case 2:
                            return CameraController.State.tight;                    
                        default:
                            return CameraController.State.standard;                    
                    }
                }                
            }
        }
    }
}
