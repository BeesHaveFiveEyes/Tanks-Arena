using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameManager : MonoBehaviour
{
    // Is the game paused?
    public bool paused;

    // Is a transition in progress?
    public bool transitionInProgress;

    // Is a preview showing?
    public bool showingPreview;

    public bool InputLocked
    {
        get
        {
            return showingPreview || paused;
        }
    }

    // Game details
    // public bool multiplayer;
    // public int difficulty;
    public int lives;
    public static int player1Score;
    public static int player2Score;
    public static int roundsPlayed;

    // Player materials
    public Material singlePlayerMaterial;
    public Material[] multiplayerMaterials;

    // Level details
    public int currentSinglePlayerLevel;    

    // Controls
    private PlayerControls controls;

    // Gameplay UI Manager
    public GameplayUIManager gameplayUIManager;

    // Camera controller
    public CameraController cameraController;

    // Initialisation properties
    private static bool initialised = false;
    private static GameManager staticCurrentGameManager;
    public bool preparingForNewRound;

    // The current GameManager instance
    public static GameManager current {
        get {
            if (!initialised) {
                staticCurrentGameManager =  FindObjectOfType<GameManager>();
                initialised = true;
            }
            return staticCurrentGameManager;
        }        
    }

    // ---------
    // Overrides
    // ---------

    [Header("Overrides")]

    [SerializeField] private bool overrideSinglePlayerLevel;
    [SerializeField] private int singlePlayerLevelOverride;

    [SerializeField] private bool overrideDifficulty;
    [SerializeField] private int difficultyOverride;

    [SerializeField] private bool overrideMultiplayer;
    [SerializeField] private bool multiplayerOverride;

    // ---------
    // Functions
    // ---------

    // Perform initial setup

    private void Start()
    {
        // Hide the cursor
        Cursor.visible = false; 
        
        // Apply single player level override
        if (overrideSinglePlayerLevel)
        {
            currentSinglePlayerLevel = singlePlayerLevelOverride;
        }

        // Apply multiplayer override
        if(overrideMultiplayer)
        {
            Preferences.context = multiplayerOverride ? Context.multiplayer : Context.singlePlayer;
        }
        
        initialised = false;
        Initialise();    
        StartPreview();      
    }

    // Perform initial setup

    private void Initialise()
    {
        Preferences.ResetStatistics();
        player1Score = 0;
        player2Score = 0;
        roundsPlayed = 0;

        InitialiseLives();  

        // Load the relevant level
        PrepareNewRound();
    }

    // Prepare for a new round

    private void PrepareNewRound()
    {        
        if (Preferences.context == Context.multiplayer)
        {
            CreateMultiplayerLevel();
        }
        else
        {
            CreateSinglePlayerLevel();
        }
        preparingForNewRound = false;
    }

    // Create the next single player level

    private void CreateSinglePlayerLevel()
    {        
        Level nextLevel = LevelManager.SinglePlayerLevels()[currentSinglePlayerLevel % LevelManager.SinglePlayerLevels().Count];        
        Preferences.highestLevelReached = nextLevel.levelName;
        Arena.CreateArena(nextLevel);        
    }

    // Choose and create the next multiplayer level

    private void CreateMultiplayerLevel()
    {           
        Level nextLevel = Arena.currentArena?.level;

        if (LevelManager.AllMultiplayerLevels(true).Count == 0)
        {
            Debug.LogError("Either no levels exists or none are marked as usable.");
            return;
        }
        else if (LevelManager.AllMultiplayerLevels(true).Count > 1)
        {
            while (nextLevel == Arena.currentArena?.level)
            {
                nextLevel = LevelManager.AllMultiplayerLevels(true)[UnityEngine.Random.Range(0, LevelManager.AllMultiplayerLevels(true).Count)];                
            }
        }
        else
        {
            nextLevel = LevelManager.AllMultiplayerLevels(true)[0];
        }

        Arena.CreateArena(nextLevel);        
    }    

    // Set the starting number of lives
    private void InitialiseLives()
    {
        // Set the corresponding number of lives
        if (Preferences.difficulty.value == 1 || Preferences.difficulty.value == 2)
        {
            lives = 3;
        }
        else
        {
            lives = 1;
        }
    }

    // Handle the death of a tank

    public void HandleAIDeath(Tank tank)
    {
        if (Preferences.context == Context.multiplayer)
        {
            Debug.LogError("HandelAIDeath() called in Single Player");
        }
        else
        {
            if (Arena.AITanks.Count == 0)
            {
                if (!preparingForNewRound)
                {
                    preparingForNewRound = true;
                    EndSinglePlayerRound();
                }                
            }
        }            
    }

    // Handle the death of a tank

    public void HandlePlayerDeath(PlayerTank tank)
    { 
        if (Preferences.context == Context.multiplayer)
        {
            if (Arena.playerTanks.Count > 0)
            {
                if (!preparingForNewRound)
                {
                    preparingForNewRound = true;
                    EndMultiplayerRound();
                }                
            }            
        }
        else
        {
            if (!preparingForNewRound)
            {
                preparingForNewRound = true;
                EndSinglePlayerRound();
            }            
        }
    }

    private void EndMultiplayerRound()
    {       
        roundsPlayed += 1; 
        Arena.currentArena.StartCoroutine(Action());
        
        IEnumerator Action()
        {
            // Pause for 2 seconds
            yield return new WaitForSeconds(2);

            // Update the scores if one player is still alive
            if (Arena.playerTanks.Count > 0)
            {
                PlayerTank playerTank = Arena.playerTanks[0];                

                if (playerTank.playerIndex == 0)
                {
                    player1Score++;
                }
                else
                {
                    player2Score++;
                }
            }

            if (roundsPlayed >= Preferences.MultiplayerDuration)
            {
                gameplayUIManager.FadeOutToEnding();
            }
            else
            {
                // Start the next round            
                Action prepareNewRound = delegate { PrepareNewRound(); };            
                gameplayUIManager.ShowTransition(prepareNewRound); 
            }            
        }
    }

    // End the single player round, after a pause

    private void EndSinglePlayerRound()
    {
        Arena.currentArena.StartCoroutine(Action());

        IEnumerator Action()
        {
            // Pause for 2 seconds
            yield return new WaitForSeconds(2);

            // If the player has died (possibly during those 2 seconds)
            if (Arena.playerTanks.Count == 0)
            {
                LoseSinglePlayerRound();
            }
            else
            {
                WinSinglePlayerRound();
            }
        }        
    }

    private void WinSinglePlayerRound()
    {
        // Add more lives if on easy difficulty and passed enough levels
        if (Preferences.difficulty.value == 1 && (currentSinglePlayerLevel + 1) % 3 == 0 && lives < 3)
        {
            lives += 1;
        }

        currentSinglePlayerLevel++;
        
        int l = LevelManager.SinglePlayerLevels().Count;
        
        if (currentSinglePlayerLevel < l)
        {
            Action prepareNewRound = delegate { PrepareNewRound(); };            
            gameplayUIManager.ShowTransition(prepareNewRound); 
        }
        else
        {                  
            SinglePlayerVictory();
        }        
    }

    private void LoseSinglePlayerRound()
    {

        if (Preferences.difficulty.value != 0)
        {
            lives -= 1;
        }

        if (lives > 0)
        {            
            Action prepareNewRound = delegate { PrepareNewRound(); };            
            gameplayUIManager.ShowTransition(prepareNewRound); 
        }
        else
        {            
            SinglePlayerDefeat();
        }
    }

    private void SinglePlayerVictory()
    {
        // Inform the ending scene of the victory
        Preferences.singlePlayerGameCompleted = true;        

        // Transition
        gameplayUIManager.FadeOutToEnding();
    }

    private void SinglePlayerDefeat()
    {
        // Inform the ending scene of the defeat
        Preferences.singlePlayerGameCompleted = false;

        // Transition
        gameplayUIManager.FadeOutToEnding();
    }

    private void StartPreview()
    {    
        showingPreview = true;
        if (Preferences.context == Context.multiplayer)
        {
            cameraController.SetMode(CameraController.State.orbittingTanks);
        }        
        if (Preferences.context == Context.singlePlayer)
        {
            cameraController.SetMode(CameraController.State.orbittingTanks);
        }  
        AudioManager.Play("Preview Music");
        gameplayUIManager.pressToStartPrompt.Show();
    }

    private void EndPreview()
    {
        if (showingPreview)
        {            
            AudioManager.FadeOutAll(2);
            // AudioManager.Play("Menu Success");
            Action action = delegate
            {                                
                cameraController.SetMode(Preferences.cameraState);
                showingPreview = false;                
            };
            
            gameplayUIManager.pressToStartPrompt.Dismiss();
            gameplayUIManager.ShowTransition(action, delegate { AudioManager.ResumeMusic(); }); 
        }        
    }

    // -------------------
    // Controls management
    // -------------------

    // Create controls    

    private void CreateControls()
    {
        controls = new PlayerControls();
        controls.Gameplay.Pause.started += ctx => gameplayUIManager.pauseMenu.Toggle();
        controls.Gameplay.Start.started += ctx => EndPreview();
    }

    // Enable controls on enable

    private void OnEnable()
    {
        CreateControls();
        controls.Gameplay.Enable();
    }

    // Disable controls on disable

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }
}
