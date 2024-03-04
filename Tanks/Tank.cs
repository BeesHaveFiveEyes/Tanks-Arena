using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{    
    public enum MovementDirection {
        forward,
        backward
    }

    public enum TurningDirection {
        clockwise,
        anticlockwise
    }

    // Parts of the tank
    [SerializeField] private GameObject turret;
    [SerializeField] private List<GameObject> wheels;
    public GameObject turretMarker;    

    // Speed floats
    public float drivingSpeed = 5;
    public float rotationSpeed = 150;
    private float wheelSpeed = 1500;        

    // Bullet properties
    public float bulletSpeed = 10;
    public float rocketSpeed = 10;
    public int bulletBounces = 3;

    // Overrides
    public bool invincible = false;

    // How close can the tank get to impassable objects
    public float proximityTolerance = 0.6f;

    // Audio properties
    [SerializeField] private float engineVolume = 0.5f;
    [SerializeField] private AudioSource engineNoiseSource;
    private float turningAudioVolume;
    private float movementAudioVolume;
    private float currentVolume;

    private void Awake()
    {        
        // Randomize rotation
        transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f,360f), 0);

        // Add this tank to the Arena's lists

        Arena.tanks.Add(this);

        if (GetComponent<PlayerTank>() == null)
        {
            Arena.AITanks.Add(this);
        }
    }  

    private void Update()
    {
        HandleAudio();
    }  

    // -------------------
    // Collision Detection
    // -------------------

    // Detect collisions with projectiles
    private void OnTriggerEnter(Collider other)
    {
        // Hit by a bullet
        if(other.gameObject.GetComponent<Bullet>() != null)
        {
            other.gameObject.GetComponent<Bullet>().Die();
            Destroy(other.gameObject);
            Die();
        }

        // Hit by a rocket
        if (other.gameObject.GetComponent<Rocket>() != null)
        {
            if (other.gameObject.GetComponent<Rocket>().tank != this)
            {
                other.gameObject.GetComponent<Rocket>().Die();
                Destroy(other.gameObject);
                Die();
            }            
        }
    }

    // ------------------
    // Shooting Functions
    // ------------------

    // Shoot a bullet with the standard number of bounces
    public void Shoot()
    {        
        Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.bullet), bulletBounces);
    }

    // Shoot a bullet with 'b' bounces
    public void Shoot(int b)
    {        
        Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.bullet), b);
    }

    // Shoot the specified projectile with the standard number of bounces
    public void Shoot(GameObject projectile)
    {        
        Shoot(projectile, bulletBounces);
    }

    // Shoot the specified projectile with 'b'' bounces
    public void Shoot(GameObject projectile, int b)
    {                
        GameObject shotProjectile = Instantiate
        (
            projectile,
            turretMarker.transform.position,
            turretMarker.transform.rotation
        );

        if (shotProjectile.GetComponent<Bullet>() != null)
        {
            Bullet bullet = shotProjectile.GetComponent<Bullet>();
            bullet.owner = this;
            bullet.bounces = b;
            bullet.bulletSpeed = bulletSpeed;
            bullet.id = UnityEngine.Random.Range(0, 100);            
        }

        if (shotProjectile.GetComponent<Rocket>() != null)
        {
            Rocket rocket = shotProjectile.GetComponent<Rocket>();
            rocket.tank = this;
            rocket.rocketSpeed = rocketSpeed;
        }

        shotProjectile.transform.parent = Arena.currentArena.transform;

        // TODO:
        //GameController.audioManager.Play("Launch");
        //GameController.audioManager.Play("Thud");
    }    

    // --------------------------
    // Movement Control Functions
    // --------------------------

    // Drive the dank in the specified direction
    public void Drive(Vector2 direction, float magnitude) {        
        
        Vector3 offset = magnitude * WorldDirectionFrom(direction) * Time.deltaTime * drivingSpeed;            

        Vector3 newPosition = transform.position;
        
        Vector3 newX = newPosition + new Vector3(offset.x, 0, 0);
        if (!Arena.Impassable(newX, proximityTolerance))
        {
            newPosition = newX;
        }

        Vector3 newZ = newPosition + new Vector3(0, 0, offset.z);
        if (!Arena.Impassable(newZ, proximityTolerance))
        {
            newPosition = newZ;
        }        

        transform.position = newPosition;        

        movementAudioVolume = magnitude;    
    }

    // Drive the tank along the direction it is currently facing
    public void Drive(MovementDirection movementDirection, float magnitude)
    {
        int multiplier = movementDirection == MovementDirection.forward ? 1 : -1;
        Vector2 direction = MapDirectionFrom(transform.forward * multiplier);
        Drive(direction, magnitude);
    }

    // Turn in the specified direction    
    public void TurnInDirection(TurningDirection turningDirection, float magnitude)
    {
        int multiplier = turningDirection == TurningDirection.clockwise ? 1 : -1;
        float rotateAmount = Time.deltaTime * multiplier * magnitude * rotationSpeed;
        Vector3 rotationVector = new Vector3(0, rotateAmount, 0);
        transform.rotation *= Quaternion.Euler(rotationVector);

        turningAudioVolume = magnitude;
    }

    // Turn towards the specified direction
    public void TurnTowards(Vector2 direction) {
        Vector3 worldDirection = WorldDirectionFrom(direction);
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, worldDirection, 0.1f * Time.deltaTime * rotationSpeed, 0);
        transform.rotation = Quaternion.LookRotation(newDirection, transform.up);   
           
        turningAudioVolume = 1;  
    }

    // --------------
    // Death Function
    // --------------

    // Handle tank death
    public void Die()
    {                
        // Create explosion
        Instantiate
        (
            PrefabManager.Prefab(PrefabManager.PrefabType.explosion),
            transform.position,
            transform.rotation
        );        

        // Create tank remnants
        // Instantiate
        // (
        //     PrefabManager.Prefab(PrefabManager.PrefabType.tankRemnants),
        //     transform.position,
        //     transform.rotation
        // );    

        // Leave scorch marks
        GameObject scorchMarks = Instantiate(PrefabManager.Prefab(PrefabManager.PrefabType.scorching));        
        Vector3 pos = transform.position;
        pos.y = 0.01f;
        scorchMarks.transform.position = pos;
        scorchMarks.transform.parent = Arena.currentArena.transform;
        Vector3 scal = scorchMarks.transform.localScale;
        scorchMarks.transform.localScale = Vector3.zero;
        LeanTween.scale(scorchMarks, scal, 0.1f);

        if (!invincible)
        {
            // Inform the GameManager and update Arena lists               
            if (TryGetComponent(out PlayerTank playerTank))
            {
                Preferences.totalPlayerDeaths++;
                Arena.tanks.Remove(this);
                Arena.playerTanks.Remove(playerTank);
                GameManager.current.HandlePlayerDeath(playerTank);
                CameraController.trackingPaused = true;
            }
            else
            {
                Preferences.tanksDefeated++;
                Arena.tanks.Remove(this);
                Arena.AITanks.Remove(this);
                GameManager.current.HandleAIDeath(this);
            }

            // Destroy the tank
            Destroy(gameObject);        
        }        
        
        AudioManager.Play("Tank Collision");
    }

    // --------------
    // Visual Styling
    // --------------

    public void SetAccentMaterial(Material material)
    {
        // turret.GetComponent<MeshRenderer>().materials[0] = material;
        turret.GetComponent<MeshRenderer>().material = material;

        foreach (GameObject wheel in wheels)
        {
            MeshRenderer meshRenderer = wheel.GetComponent<MeshRenderer>();
            Material[] wheelMaterials = meshRenderer.sharedMaterials;
            wheelMaterials[2] = material;
            meshRenderer.sharedMaterials = wheelMaterials;
        }
    }

    // -----
    // Audio
    // -----

    private void HandleAudio()
    {
        if (engineNoiseSource != null)
        {
            if (Preferences.soundEffects.value)
            {
                float targetVolume = engineVolume * Math.Max(turningAudioVolume, movementAudioVolume);
                turningAudioVolume = 0;
                movementAudioVolume = 0;

                currentVolume += 12 * Time.deltaTime * (targetVolume - currentVolume);

                engineNoiseSource.volume = currentVolume;
            }
            else
            {
                engineNoiseSource.volume = 0;
            }
        }
    }

    // ----------------
    // Helper Functions
    // ----------------

    // Converts a Vector2 direction to a Vector3 in the equivalent direction
    public Vector3 WorldDirectionFrom(Vector2 vector2) {
        return new Vector3(-vector2.y, 0, vector2.x).normalized;
    }

    public Vector3 MapDirectionFrom(Vector3 vector3) {
        return new Vector2(vector3.z, -vector3.x).normalized;
    }

    // private void Update()
    // {
    //     // Aiming the turret:

    //     if (controllingTurret && turretAim != Vector3.zero)
    //     {
    //         Vector3 newTurretDirection = Vector3.RotateTowards(turret.transform.forward, turretAim, turretRotationSpeed * Time.deltaTime, 0);
    //         turret.transform.rotation = Quaternion.LookRotation(newTurretDirection);
    //     }

    //     // Spinning the wheels:

    //     foreach (GameObject wheel in wheels)
    //     {
    //         wheel.transform.Rotate(transform.up, input.magnitude * wheelSpeed * Time.deltaTime);
    //     }

    //     if (easyMovement)
    //     {
    //         Vector3 positionAddition = input * Time.deltaTime * tankSpeed;
    //         Vector3 newPosition = transform.position;

    //         Vector3 newX = newPosition + new Vector3(positionAddition.x, 0, 0);

    //         if (!Arena.Impassable(newX, proximityTolerance))
    //         {
    //             newPosition = newX;
    //         }

    //         Vector3 newZ = newPosition + new Vector3(0, 0, positionAddition.z);
    //         if (!Arena.Impassable(newZ, proximityTolerance))
    //         {
    //             newPosition = newZ;
    //         }

    //         if (driving)
    //         {
    //             Vector3 newDirection = Vector3.RotateTowards(transform.forward, newPosition - transform.position, 0.1f * Time.deltaTime * tankRotationSpeed, 0);
    //             transform.rotation = Quaternion.LookRotation(newDirection, transform.up);
    //         }            

    //         transform.position = newPosition;            
    //     }

    //     else
    //     {
    //         // Rotating the tank:
    //         float rotateAmount = Time.deltaTime * input.x * tankRotationSpeed;
    //         Vector3 rotationVector = new Vector3(0, rotateAmount, 0);
    //         transform.rotation *= Quaternion.Euler(rotationVector);

    //         Vector3 direction = input.normalized;

    //         // TODO:

    //         //if (input.magnitude > 0.01f)
    //         //{            
    //         //    if (!playingAudio)
    //         //    {
    //         //        playingAudio = true;
    //         //        driveSound.Stop();
    //         //        driveSound.Play();                
    //         //    }

    //         //    driveSound.volume += 0.05f * 1.6f - driveSound.volume;
    //         //}
    //         //else
    //         //{
    //         //    playingAudio = false;
    //         //    driveSound.volume *= 0.95f;
    //         //}

    //         // Moving the tank:

    //         Vector3 positionAddition = transform.forward * Time.deltaTime * tankSpeed * input.z;
    //         Vector3 newPosition = transform.position;

    //         Vector3 newX = newPosition + new Vector3(positionAddition.x, 0, 0);

    //         if (!Arena.Impassable(newX, proximityTolerance))
    //         {
    //             newPosition = newX;
    //         }

    //         Vector3 newZ = newPosition + new Vector3(0, 0, positionAddition.z);
    //         if (!Arena.Impassable(newZ, proximityTolerance))
    //         {
    //             newPosition = newZ;
    //         }

    //         transform.position = newPosition;

    //         // TODO:
    //         //if (PlayerPrefs.HasKey("soundEffects"))
    //         //{
    //         //    if (PlayerPrefs.GetInt("soundEffects") == 0)
    //         //    {
    //         //        driveSound.volume = 0;
    //         //    }
    //         //}
    //     }
    // }
}
