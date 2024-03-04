using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartsDisplay : MonoBehaviour
{
    [SerializeField]
    public GameObject[] hearts;

    // Update the hearts display showing the remaining lives
    public void Configure()
    {
        bool active = Preferences.difficulty.value != 0 && !(Preferences.context == Context.multiplayer);
        gameObject.SetActive(active);

        for (int i = 0; i < 3; i++)
        {
            hearts[i].SetActive(GameManager.current.lives > i);
        }
    }
}
