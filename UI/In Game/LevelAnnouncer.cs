using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelAnnouncer : MonoBehaviour
{
    // ----------
    // UI Objects
    // ----------

    public GameObject[] singlePlayerElements;
    public GameObject[] multiPlayerElements;

    public TextMeshProUGUI levelTitle;

    public TextMeshProUGUI player1Name;
    public TextMeshProUGUI player2Name;

    public TextMeshProUGUI player1Score;
    public TextMeshProUGUI player2Score;

    public float slideTime = 0.5f;
    public float pause = 2f;

    public UISlider leftPanel;
    public UISlider rightPanel;    

    public HeartsDisplay heartsDisplay;

    // Immediately dismiss the level announcer
    public void Clear()
    {
        leftPanel.gameObject.SetActive(false);
        rightPanel.gameObject.SetActive(false);
    }

    // Set up all the relevant children
    private void ConfigureElements()
    {
        levelTitle.text = "LEVEL " + (GameManager.current.currentSinglePlayerLevel + 1).ToString();    
        player1Score.text = GameManager.player1Score.ToString();
        player2Score.text = GameManager.player2Score.ToString();    

        bool multiplayer = Preferences.context == Context.multiplayer;

        foreach (GameObject element in singlePlayerElements)
        {
            element.SetActive(!multiplayer);
        }

        foreach (GameObject element in multiPlayerElements)
        {
            element.SetActive(multiplayer);
        }

        heartsDisplay.Configure();
    }

    // Transition between rounds
    // Only called if another round is expected

    public void Transition()
    {
        ConfigureElements();        

        AudioManager.Play("Slider"); 

        // Slide in the left and right panels
        rightPanel.Show(slideTime);
        leftPanel.Show(slideTime);

        StartCoroutine(RevealNextLevel());

        IEnumerator RevealNextLevel()
        {
            yield return new WaitForSeconds(pause);

            AudioManager.Play("Slider"); 
            rightPanel.Dismiss(slideTime);
            leftPanel.Dismiss(slideTime);
        }
    }
}

