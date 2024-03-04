using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Windows;

public class PlayerTank : MonoBehaviour
{   
    public enum ControlStyle
    {        
        // With simple controls, the tank drives in the direction indicated
        // With manual controls, the tank drives in the direction it is facing

        simple,
        manual
    }

    // The associated Tank component
    public Tank tank;    

    // The index of the tank
    [HideInInspector] public int playerIndex;

    // ----------------------
    // Player Properties
    // ----------------------

    // The maximum number of bullets the player can carry
    public int bulletCap;

    // The time taken for the player tank to replenish its bullets
    public float bulletReplenishmentTime;       

    // The time taken for the player tank to replenish its bullets
    private float rapidFireDelay = 0.05f;                  

    // ---------------
    // Live Properties
    // ---------------

    // The current powerup possessed by the tank
    private PowerUp powerUpType;   

    // Player controls properties
    // [SerializeField] private PlayerInput playerInput;
    // private PlayerControls controls;
    // private InputAction simpleMovementAction;
    // private InputAction manualTurningAction;
    // private InputAction manualDrivingAction;
    public ControlStyle controlStyle;     

    // Current projectile counts
    private int bulletsRemaining = 0;   
    private int rocketsRemaining = 3;
    private int rapidFireBulletsRemaining = 30;   

    // Local control properties
    private bool shootingPerformed;
    private bool simpleDrivingPerformed = false;    
    private bool simpleForwardPerformed = false;
    private bool simpleReversePerformed = false;
    private bool manualDrivingPerformed = false;
    private bool manualTurningPerformed = false;
    private Vector2 simpleDirectionInput;   
    private float simpleForwardInput;
    private float simpleReverseInput; 
    private float manualDrivingInput;
    private float manualTurningInput;

    // Local cooldowns       
    private float rapidFireCooldown = -1;
    private float bulletReplenishmentCooldown = -1;  

    // The tanks shield, if one is present
    private Shield shield;

    private void Awake()
    {   
        // Add this tank to the Arena's list of player tanks
        Arena.playerTanks.Add(this); 
    }

    private void Start()
    {               
        // Initialise bullet count
        bulletsRemaining = bulletCap;

        // Initialise cooldowns
        bulletReplenishmentCooldown = bulletReplenishmentTime;

        // TODO: Add other cooldowns here?        
    }

    public void OnSimpleAiming(InputAction.CallbackContext ctx)
    {
        // Handle aiming under simple controls
        if (controlStyle == ControlStyle.simple)
        {
            simpleDrivingPerformed = ctx.performed && !GameManager.current.InputLocked;
            simpleDirectionInput = ctx.ReadValue<Vector2>();
        }
    }

    public void OnSimpleForward(InputAction.CallbackContext ctx)
    {
        // Handle forward movement under simple controls
        if (controlStyle == ControlStyle.simple)
        {
            simpleForwardPerformed = ctx.performed && !GameManager.current.InputLocked;
        }
    }

    public void OnSimpleReverse(InputAction.CallbackContext ctx)
    {
        // Handle reverse under simple controls
        if (controlStyle == ControlStyle.simple)
        {
            simpleReversePerformed = ctx.performed && !GameManager.current.InputLocked;
        }
    }

    public void OnManualTurning(InputAction.CallbackContext ctx)
    {
        // Handle turning under manual turning controls
        if (controlStyle == ControlStyle.manual)
        {
            manualTurningPerformed = ctx.performed && !GameManager.current.InputLocked;
            manualTurningInput = ctx.ReadValue<float>();
        }   
    }

    public void OnManualDriving(InputAction.CallbackContext ctx)
    {
        // Handle movement under manual turning controls
        if (controlStyle == ControlStyle.manual)
        {
            manualDrivingPerformed = ctx.performed && !GameManager.current.InputLocked;
            manualDrivingInput = ctx.ReadValue<float>();            
        }   
    }

    public void OnShooting(InputAction.CallbackContext ctx)
    {
        // Handle movement under manual turning controls
        if (ctx.performed && !GameManager.current.InputLocked) { shootingPerformed = true; Debug.Log(Arena.currentArena.level.DebugDescription());}
    }

    public void SetPowerUp(PowerUp powerUpTypeInput)
    {    
        if (shield != null)
        {
            shield.Die();
        }

        powerUpType = powerUpTypeInput;

        if (powerUpType == PowerUp.shield)
        {            
            shield = Instantiate(PrefabManager.Prefab(PrefabManager.PrefabType.shield), tank.transform).GetComponent<Shield>();
            shield.owner = tank;
        }
    }

    // Detect collisions with powerups
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PowerUpDispenser>() != null)
        {
            PowerUpDispenser powerUp = other.gameObject.GetComponent<PowerUpDispenser>();

            if (powerUp.showingPowerUp)
            {
                SetPowerUp(powerUp.currentPowerUp);                    

                // Create popup
                PowerupPopUp popUp = Instantiate
                (
                    PrefabManager.Prefab(PrefabManager.PrefabType.powerupPopUp),
                    FindObjectOfType<Canvas>().transform
                ).GetComponent<PowerupPopUp>();  
                popUp.target = transform;
                popUp.powerUpType = powerUp.currentPowerUp;

                // Reset the powerup
                powerUp.Collect();
            }            
        }
    }

    private void Update()
    {
        ManageFireCooldowns();

        if (!GameManager.current.InputLocked)
        {
            // Handle Shooting
            if (shootingPerformed)
            {
                Shoot();
            }

            // Handle movement under simple controls
            if (controlStyle == ControlStyle.simple)
            {
                // Handle Turning
                if (simpleDrivingPerformed)
                {                                        
                    // Handle Turning                        
                    tank.TurnTowards(simpleDirectionInput.normalized);
                }            

                // Handle Driving
                if (simpleForwardPerformed && !simpleReversePerformed)
                {
                    tank.Drive(simpleDirectionInput.normalized, simpleDirectionInput.magnitude);
                }
                if (simpleReversePerformed && !simpleForwardPerformed)
                {
                    tank.Drive(simpleDirectionInput.normalized * -1, simpleDirectionInput.magnitude);
                }
            }

            // Handle movement under manual turning controls
            else
            {
                // Handle Turning
                if (manualTurningPerformed)
                {                         
                    float magnitude = Mathf.Abs(manualTurningInput);             
                    Tank.TurningDirection turningDirection = manualTurningInput > 0 ? Tank.TurningDirection.clockwise : Tank.TurningDirection.anticlockwise;
                    tank.TurnInDirection(turningDirection, magnitude);
                }

                // Handle Driving
                if (manualDrivingPerformed)
                {                         
                    float magnitude = Mathf.Abs(manualDrivingInput);             
                    Tank.MovementDirection movementDirection = manualDrivingInput > 0 ? Tank.MovementDirection.forward : Tank.MovementDirection.backward;
                    tank.Drive(movementDirection, magnitude);
                }            
            }   
        }             
    }

    private void ManageFireCooldowns()
    {
        if (bulletsRemaining < bulletCap)
        {
            if (bulletReplenishmentCooldown > 0)
            {
                bulletReplenishmentCooldown -= Time.deltaTime;
            }
            else
            {
                bulletsRemaining = bulletCap;
                bulletReplenishmentCooldown = bulletReplenishmentTime;
            }
        }

        if (rapidFireCooldown > -1)   
        {
            rapidFireCooldown -= Time.deltaTime;
        }        
    }    

    private void Shoot()
    {
        // Don't shoot again next frame
        shootingPerformed = false;

        // Check there are bullets remaining (or bypass in case of rapidFire)
        if (bulletsRemaining > 0 || powerUpType == PowerUp.rapidFire)
        {
            // Rockets
            if (powerUpType == PowerUp.rocket)
            {
                tank.Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.rocket));
                rocketsRemaining--;

                if (rocketsRemaining == 0)
                {
                    powerUpType = PowerUp.none;
                    rocketsRemaining = 3;
                }
            }

            //Homing Rockets
            else if (powerUpType == PowerUp.homingRocket)
            {
                tank.Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.homingRocket));
                rocketsRemaining--;

                if (rocketsRemaining == 0)
                {
                    powerUpType = PowerUp.none;
                    rocketsRemaining = 3;
                }
            }

            // Rapid Fire
            else if (powerUpType == PowerUp.rapidFire)
            {
                // Bypass the usual shooting cancellation
                shootingPerformed = true;
                
                if (rapidFireCooldown < 0)
                {
                    // Shoot a bullet with one bounce only
                    tank.Shoot(1);

                    rapidFireBulletsRemaining--;

                    if (rapidFireBulletsRemaining == 0)
                    {
                        powerUpType = PowerUp.none;
                        rapidFireBulletsRemaining = 30;
                    }

                    rapidFireCooldown = rapidFireDelay;
                }
            }

            // Bullets (No powerup)
            else
            {
                tank.Shoot();
                bulletsRemaining--;
            }
        }
        else
        {            
            AudioManager.Play("Thud");
        }
    }

    public void RemovePowerUp()
    {
        powerUpType = PowerUp.none;
    }
}
