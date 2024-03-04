using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenuManager : MonoBehaviour
{
    [Serializable]
    public enum NavigationState
    {
        titleScreen,
        singlePlayer,
        multiplayer,
        arenaEditor
    }    

    public View[] rootViews;
    public View arenaLibraryView;
    
    public ViewInputController inputController;

    public NavigationState navigationState;

    public GameObject multiplayerScreenArenaList;
    public GameObject arenaEditorScreenCustomArenaList;
    public GameObject arenaEditorScreenOfficialArenaList;
    private List<GameObject> addedArenaElements = new();

    public UIFader loadingScreen;
    public UIFader transitionFader;
    public UIFader mainFader;

    public Alert alert;

    public float loadingScreenTime = 7;

    // Prefabs
    [SerializeField] private GameObject arenaRowControlPrefab;
    [SerializeField] private GameObject arenaRowItemPrefab;
    
    private void Start()
    {
        // Load in levels
        LevelManager.ReadLevelsFromFiles();
        
        // Hide the cursor
        Cursor.visible = false; 
        
        // Hide all root views that are not the current selected root view
        if (Preferences.lastMainMenuRootView != "'")
        {
            View targetView = Array.Find(rootViews, view => view.viewName == Preferences.lastMainMenuRootView);
            if (targetView != null)
            {
                inputController.rootView = targetView;
                targetView.Refresh();
            }
        } 
        if (Preferences.lastMainMenuPreviousView != "'")
        {
            View targetView = Array.Find(rootViews, view => view.viewName == Preferences.lastMainMenuPreviousView);
            if (targetView != null)
            {
                inputController.previousView = targetView;
                inputController.escapeAction = (View view) => TransitionTo(view.name, false);
            }
        }        

        for (int i = 0; i < rootViews.Length; i++)
        {
            if (rootViews[i] != inputController.rootView)
            {
                rootViews[i].gameObject.SetActive(false);
            }
            else
            {
                rootViews[i].gameObject.SetActive(true);
            }
        }

        LoadLevels();

        loadingScreen.gameObject.SetActive(false);
        transitionFader.gameObject.SetActive(true);
        transitionFader.Dismiss();
    }

    public void TransitionTo(string target)
    {                
        TransitionTo(target, false);
    }

    public void ReversablyTransitionTo(string target)
    {                
        TransitionTo(target, true);
    }

    private void TransitionTo(string target, bool reversable)
    {                
        View targetView = Array.Find(rootViews, view => view.viewName == target);
        
        if (targetView == null)
        {
            Debug.LogError("View named '" + target + "' not found.");
        }
        else if (targetView != inputController.rootView)
        {
            AudioManager.Play("Menu Confirmation");

            // Hide the current view
            if (inputController.rootView.TryGetComponent(out UISlider slider))
            {
                slider.Dismiss();
            }
            else
            {
                inputController.rootView.gameObject.SetActive(false);
            }            

            // Show the new view
            if (targetView.TryGetComponent(out UISlider slider1))
            {
                slider1.Show();
            }
            else
            {
                targetView.gameObject.SetActive(true);
            }            
                        
            inputController.escapeAction = (View view) => TransitionTo(view.name, false);
            inputController.SetRootView(targetView, reversable);
        }
    }

    private void LoadLevels()
    {
        foreach (GameObject item in addedArenaElements)
        {
            item.gameObject.SetActive(false);            
            Destroy(item);
        }

        addedArenaElements = new();

        LevelManager.ReadLevelsFromFiles();
        AddLevelControlsTo(multiplayerScreenArenaList.transform);
        AddLevelItemsTo(arenaEditorScreenCustomArenaList.transform, true);
        AddLevelItemsTo(arenaEditorScreenOfficialArenaList.transform, false);
        
        if (arenaLibraryView.gameObject.activeInHierarchy)
        {
            arenaLibraryView.Refresh();
        }        
    }

    private void AddLevelControlsTo(Transform transform)
    {        
        List<Level> levels = LevelManager.AllMultiplayerLevels(false);
        foreach (Level level in levels)
        {
            GameObject control = Instantiate(arenaRowControlPrefab, transform);
            addedArenaElements.Add(control);
            ArenaRowControl arenaRowControl = control.GetComponent<ArenaRowControl>();
            arenaRowControl.level = level;
            arenaRowControl.Initialise();
        }        
    }

    private void AddLevelItemsTo(Transform transform, bool custom)
    {        
        List<Level> levels = custom ? LevelManager.CustomMultiplayerLevels(false) : LevelManager.OfficialMultiplayerLevels(false);
        foreach (Level level in levels)
        {                        
            GameObject item = Instantiate(arenaRowItemPrefab, transform);                        
            addedArenaElements.Add(item);            
            ArenaRowElement arenaRowElement = item.GetComponent<ArenaRowElement>();
            arenaRowElement.level = level; 
            arenaRowElement.Initialise();
        }        
    }

    private void HideViewsForLaunch()
    {
        for (int i = 0; i < rootViews.Length; i++)
        {            
            if (rootViews[i].gameObject.activeInHierarchy)
            {                
                if (rootViews[i].TryGetComponent(out UISlider slider))
                {
                    slider.SetSlideType(UISlider.SlideType.left);
                    slider.Dismiss();
                }
                else
                {
                    rootViews[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void LaunchSinglePlayerGame()
    {
        Preferences.context = Context.singlePlayer;
        LoadScene("LevelPlayer");
    }

    public void LaunchMultiplayerGame()
    {
        Preferences.context = Context.multiplayer;
        LoadScene("LevelPlayer");
    }

    public void LaunchLevelEditor(Context context)
    {
        Preferences.context = context;
        LoadSceneWithoutLoadingScreen("LevelEditor");
    }

    public void CreateNewLevel()
    {
        Preferences.context = Context.mp_editor_newLevel;
        LoadSceneWithoutLoadingScreen("LevelEditor");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DeleteLevel(string levelName)
    {
        LevelManager.DeleteCustomLevel(levelName);
        LoadLevels();
        inputController.rootView.Refresh();
    }

    private void LoadSceneWithoutLoadingScreen(string key)
    {
        Preferences.lastMainMenuRootView = "Arena Library";
        Preferences.lastMainMenuPreviousView = "Title Screen";
        inputController.locked = true;
        HideViewsForLaunch();        

        void Transition()
        {            
            SceneManager.LoadScene(key);
        }

        mainFader.Show(0.5f, Transition);
        
        AudioManager.Play("Menu Success");
        AudioManager.PauseMusic();    
    }

    private void LoadScene(string key)
    {
        Preferences.lastMainMenuRootView = "";
        Preferences.lastMainMenuPreviousView = "";

        inputController.locked = true;

        HideViewsForLaunch();

        loadingScreen.Show();

        void Transition()
        {            
            void Load()
            {
                SceneManager.LoadScene(key);
            }

            transitionFader.Show(-1, Load);
        }

        LeanTween.delayedCall(loadingScreenTime, Transition);
        
        AudioManager.Play("Menu Success");
        AudioManager.PauseMusic();        
    }
}
