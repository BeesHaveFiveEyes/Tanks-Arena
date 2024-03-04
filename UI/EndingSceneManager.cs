using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSceneManager : MonoBehaviour
{
    public UIFader fader;
    
    public GameObject singlePlayerVictorySlide;
    public GameObject singlePlayerDefeatSlide;
    public GameObject multiplayerSlide;    

    public GameObject player1VictoryImage;
    public GameObject player1DefaultImage;
    public GameObject player2VictoryImage;
    public GameObject player2DefaultImage;

    // Start is called before the first frame update
    void Start()
    {
        singlePlayerVictorySlide.SetActive(false);
        singlePlayerDefeatSlide.SetActive(false);
        multiplayerSlide.SetActive(false);

        if (Preferences.context == Context.singlePlayer)
        {
            if (Preferences.singlePlayerGameCompleted)
            {
                singlePlayerVictorySlide.SetActive(true);
                AudioManager.Play("Victory");
            }
            else
            {
                singlePlayerDefeatSlide.SetActive(true);
                AudioManager.Play("Defeat");
            }
        }   
        else if (Preferences.context == Context.multiplayer)
        {
            multiplayerSlide.SetActive(true);
            
            // Show appropriate player images
            bool player1Won = GameManager.player1Score > GameManager.player2Score;
            bool player2Won = GameManager.player1Score < GameManager.player2Score;
            player1VictoryImage.SetActive(player1Won);
            player1DefaultImage.SetActive(!player1Won);
            player2VictoryImage.SetActive(player2Won);
            player2DefaultImage.SetActive(!player2Won);

            AudioManager.Play("Victory");
        }   
        else
        {
            Debug.LogWarning("Ending Scene launched in invalid context");
        } 

        fader.gameObject.SetActive(true);
        fader.Dismiss();
    }

    // Fade out to black and return to the main menu
    public void ToMainMenu()
    {        
        AudioManager.FadeOutAll();

        void Continue()
        {
            SceneManager.LoadScene("MainMenu");
        }

        fader.Show(1, Continue);
    }
}
