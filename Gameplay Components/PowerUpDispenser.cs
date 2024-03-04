using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpDispenser : MonoBehaviour
{    
    // The current power up showing 
    [HideInInspector] public PowerUp currentPowerUp;

    // Rotation and spawn frequency properties
    [SerializeField] private float rotationSpeed = 150;
    [SerializeField] private float spawnTimeMinimum = 2;
    [SerializeField] private float spawnTimeMaximum = 20;    

    // Is the powerup box showing?
    [HideInInspector] public bool showingPowerUp;

    // The powerup box gameobject
    [SerializeField] private GameObject powerUpBox;

    // A timer for powerup spawning
    private float spawnTimer;

    // A record of the initial powerup box scale
    private Vector3 initialScale;
    
    // A list of the currently enabled powerups
    private List<PowerUp> enabledPowerUps
    {
        get
        {
            List<PowerUp> output = new();
            
            if (Preferences.rockets.value)
            {
                output.Add(PowerUp.rocket);
            }

            if (Preferences.homingRockets.value)
            {
                output.Add(PowerUp.homingRocket);
            }

            if (Preferences.rapidFire.value)
            {
                output.Add(PowerUp.rapidFire);
            }

            if (Preferences.shields.value)
            {
                output.Add(PowerUp.shield);
            }

            return output;
        }
    }

    private void Start()
    {
        if (Preferences.Editing)
        {
            showingPowerUp = true;
            ShowBox(false);
        }
        else
        {
            Reset(false);
        }      

        initialScale = powerUpBox.transform.localScale;  
    }

    void Update()
    {
        // Spin the powerup box
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Handle the spawn cooldown
        if (!showingPowerUp && enabledPowerUps.Count > 0)
        {
            if (!GameManager.current.paused && !GameManager.current.showingPreview)
            {
                spawnTimer -= Time.deltaTime;

                if (spawnTimer < 0)
                {
                    showingPowerUp = true;
                    ShowBox(true);
                }
            }
        }        
    }

    private void ShowBox(bool animated)
    {
        powerUpBox.SetActive(true);           
        if (animated)
        {
            LeanTween.value(gameObject, 0, 1, 0.4f)
            .setOnUpdate(t => {
                powerUpBox.transform.localScale = Vector3.zero * (1 - t) + initialScale * t;
            });
        }        
    }

    private void HideBox(bool animated)
    {
        if (animated)
        {
            // Create particles
            GameObject explosion = Instantiate(
                PrefabManager.Prefab(PrefabManager.PrefabType.powerUpParticles),
                transform.position,
                Quaternion.identity
            );
        }

        powerUpBox.SetActive(false);
    }

    // Select a random powerup
    public void Randomise()
    {        
        if (enabledPowerUps.Count > 0)
        {
            int i = Random.Range(0, enabledPowerUps.Count);
            currentPowerUp = enabledPowerUps[i];
        }        
    }

    // Collect the powerup
    public void Collect()
    {
        AudioManager.Play("Item");
        AudioManager.Play("Power Up");
        Reset(true);
    }

    // Reset the powerup cooldown
    public void Reset(bool animated)
    {
        showingPowerUp = false;
        HideBox(animated);
        Randomise();
        spawnTimer = Random.Range(spawnTimeMinimum, spawnTimeMaximum);        
    }

}
