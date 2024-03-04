using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public UISlider leftSlider;
    public UISlider rightSlider;

    public UIFader fader;

    public ViewInputController viewInputController;

    // Immediately hide the pause menu
    public void Clear()
    {
        viewInputController.active = false;
        viewInputController.locked = true;
        leftSlider.gameObject.SetActive(false);
        rightSlider.gameObject.SetActive(false);
    }

    // Toggle paused / unpaused
    public void Toggle()
    {
        if (!GameManager.current.paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    // Pause the game
    public void Pause()
    {                
        if (!GameManager.current.transitionInProgress)
        {
            ConfigureControlsDisplay();

            gameObject.SetActive(true);

            viewInputController.active = true;
            viewInputController.locked = false;
            viewInputController.rootView.SelectFirstChild();

            if (!GameManager.current.paused)
            {
                AudioManager.Play("Slider"); 
                AudioManager.PauseMusic();           

                leftSlider.Show();
                rightSlider.Show();
                    
                GameManager.current.paused = true;
            }
        }
    }

    // Unpause the game
    public void Resume()
    {    
        if (!GameManager.current.transitionInProgress)
        {
            if (GameManager.current.paused)
            {            
                AudioManager.Play("Slider");
                
                if (!GameManager.current.showingPreview)
                {
                    AudioManager.ResumeMusic();
                }

                leftSlider.Dismiss();
                rightSlider.Dismiss();
                
                GameManager.current.paused = false;            
            }

            viewInputController.active = false;
            viewInputController.locked = true;
        }        
    }

    public void RestartGame()
    {
        void Continue()
        {
           SceneManager.LoadScene("LevelPlayer");
        }

        fader.Show(1, Continue);
    }

    // Transition to the main menu
    public void ToMainMenu()
    {        
        void Continue()
        {
           SceneManager.LoadScene("MainMenu");
        }

        fader.Show(1, Continue);
    }

    private void ConfigureControlsDisplay()
    {
        // TODO;
    }
}
