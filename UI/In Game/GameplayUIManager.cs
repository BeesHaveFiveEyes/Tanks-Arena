using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameplayUIManager : MonoBehaviour
{
    public UIFader fader;
    public LevelAnnouncer levelAnnouncer;    
    public PauseMenu pauseMenu;
    public UISlider pressToStartPrompt;

    private void Start() {
        
        // Fade in from black
        fader.gameObject.SetActive(true);
        fader.Dismiss(1);

        // Hide the pause menu and the level announcer
        pauseMenu.Clear();
        levelAnnouncer.Clear();
    }

    public void ShowTransition(Action action, Action completionAction = null) {

        GameManager.current.transitionInProgress = true;

        // Slide in the level announcer transition UI
        levelAnnouncer.Transition();

        // Perform the supplied action after 'slide in' is complete
        GameManager.current.StartCoroutine(DelayedAction());
        IEnumerator DelayedAction() {
            yield return new WaitForSeconds(levelAnnouncer.slideTime);
            action();
        }

        GameManager.current.StartCoroutine(SecondDelayedAction());
        IEnumerator SecondDelayedAction() {
            yield return new WaitForSeconds(2 * levelAnnouncer.slideTime + levelAnnouncer.pause);
            if (completionAction != null)
            {
                completionAction();
            }
            GameManager.current.transitionInProgress = false;
        }
    }

    public void FadeOutToEnding()
    {
        Preferences.gameDuration = Time.timeSinceLevelLoad;
        AudioManager.FadeOutAll();
        void Continue()
        {
            SceneManager.LoadScene("EndingScene");
        }

        fader.Show(1, Continue);
    }
}
