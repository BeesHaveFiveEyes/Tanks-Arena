using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AITank : MonoBehaviour
{
    // The associated Tank component
	public Tank tank;    

    // Fire rate properties
    [Header("Shooting Delay")]
    public float shootingDelayMinimum = 0.5f;
    public float shootingDelayMaximum = 0.5f;
    [Header("Burst Delay")]
    public float shootingBurstDelayMinimum = 2f;
    public float shootingBurstDelayMaximum = 2f;
    [Header("Burst Count")]
    public int shootingBurstCountMinimum = 1;
    public int shootingBurstCountMaximum = 2;   
    private float checkShootingPathDelay = 0.3f; 

    // Behaviour properties
    [Header("Behaviour Properties")]
    public bool canRotateTurret;    
    public bool patrolling;    
    public float patrolTravelDuration;
    public float patrolWaitDuration;

    //PowerUp
    public PowerUp powerUp = PowerUp.none;

    // Cooldowns    
    private float checkShootingPathCooldown = -1;
    public float shootingCooldown = -1;
    public float shootingBurstCooldown = -1;
    private float patrolCooldown;

    // Live Variables
    private Tank target;
    private int shootingBurstShotsFired = 0;
    private int liveShootingBurstCount = 0;
    private bool foundPlayer;    
    private bool readyToShoot;
    private bool movingInPatrol;
    private bool rotatingInPatrol;    
    private bool justRotated;

    private Vector3 moveStart;
    private float targetDistance;
    private Vector3 targetDirection;
    private bool rotatingClockwise;
    private float livePatrolLag; 
    public float patrolLagMinimum; 
    public float patrolLagMaximum; 

    private void Start()
    {
        // Findthe nearest player:
        target = NearestPlayer();

        // Initialising the cooldowns:
        
        // TODO

        // Setting initial live variables:

        // justRotated = false;
        // movingInPatrol = false;
        // rotatingInPatrol = false;

        // Add shield

        if(powerUp == PowerUp.shield)
        {
            Shield shield = Instantiate(PrefabManager.Prefab(PrefabManager.PrefabType.shield), tank.transform).GetComponent<Shield>();
            shield.owner = tank;
        }

        RandomiseLiveShootingBurstCount();
        ResetShootingBurstCooldown();
    }

    private void ResetCheckShootingPathCooldown()
    {
        checkShootingPathCooldown = checkShootingPathDelay;
    }

    private void ResetShootingCooldown()
    {
        shootingCooldown = Random.Range(shootingDelayMinimum, shootingDelayMaximum + 1);
    }

    private void ResetShootingBurstCooldown()
    {
        shootingBurstCooldown = Random.Range(shootingBurstDelayMinimum, shootingBurstDelayMaximum + 1);
    }

    private void RandomiseLiveShootingBurstCount()
    {
        liveShootingBurstCount = Random.Range(shootingBurstCountMinimum, shootingBurstCountMaximum + 1);
    }

    private void ResetPatrolCooldown()
    {
        livePatrolLag = Random.Range(patrolLagMinimum, patrolLagMaximum);
    }

    private void Update()
    {       
        if (!GameManager.current.InputLocked)
        {
            checkShootingPathCooldown -= Time.deltaTime;          

            if (checkShootingPathCooldown < 0)
            {
                target = NearestPlayer();

                if (target != null)
                {
                    foundPlayer = EstimateShootingPath();
                    ResetCheckShootingPathCooldown();

                    if (foundPlayer && canRotateTurret && !readyToShoot)
                    {
                        ResetShootingCooldown();
                        readyToShoot = true;                    
                    }

                    if (!foundPlayer)
                    {
                        // TODO

                        // tank.turret.transform.rotation = Quaternion.LookRotation(tank.transform.forward);
                        // tank.turretAim = Vector3.zero;                    
                        readyToShoot = false;                    
                    }
                }            
            }


            if (target != null)
            {
                if (foundPlayer)
                {
                    Attack();
                }

                if (!foundPlayer || canRotateTurret)
                {
                    Patrol();
                }
            }        
        }
    }

    private void Patrol()
    {            
        if (patrolling)
        {
            if (movingInPatrol)
            {                             
                if ((tank.transform.position - moveStart).magnitude < targetDistance)
                {
                    tank.Drive(Tank.MovementDirection.forward, 1);
                    tank.TurnInDirection(Tank.TurningDirection.clockwise, 0.2f * Mathf.Sin(5 * Time.time));
                }
                else
                {                             
                    movingInPatrol = false;
                }
            }
            else if (rotatingInPatrol)
            {                   
                int multiplier = rotatingClockwise ? 1 : -1;
                float rotateAmount = Time.deltaTime * multiplier * tank.rotationSpeed;
                Vector3 rotationVector = new Vector3(0, rotateAmount, 0);
                Vector3 newDirection = Quaternion.Euler(multiplier * rotationVector) * tank.transform.forward;

                if ((newDirection - targetDirection).magnitude
                    < (tank.transform.forward - targetDirection).magnitude)
                {
                    tank.TurnInDirection(rotatingClockwise ? Tank.TurningDirection.clockwise : Tank.TurningDirection.anticlockwise, 1);
                }
                else
                {                     
                    rotatingInPatrol = false;
                }
                
            }
            else
            {
                patrolCooldown -= Time.deltaTime;

                if (patrolCooldown < 0)
                {
                    ResetPatrolCooldown();

                    if (justRotated)
                    {
                        moveStart = tank.transform.position;
                        float maxDistance = Mathf.Max(0, WalkableDistance() - 1);                        
                        targetDistance = Random.Range(0.7f * maxDistance, maxDistance);

                        movingInPatrol = true;

                        justRotated = false;
                    }
                    else
                    {
                        targetDirection = ChooseDirection();

                        float rotateAmount = Time.deltaTime * tank.rotationSpeed;
                        Vector3 rotationVector = new Vector3(0, rotateAmount, 0);

                        Vector3 antiClockwiseRotation = Quaternion.Euler(-rotationVector) * tank.transform.forward;
                        Vector3 clockwiseRotation = Quaternion.Euler(rotationVector) * tank.transform.forward;

                        
                        rotatingClockwise = (antiClockwiseRotation - targetDirection).magnitude
                            > (clockwiseRotation - targetDirection).magnitude;

                        rotatingInPatrol = true;

                        if (patrolling)
                        {
                            justRotated = true;
                        }                    
                    }
                }
            }
        }
    }

    private void Attack()
    {                
        if (canRotateTurret)
        {
            // TODO 
            // Vector3 toPlayer = target.transform.position - transform.position;
            // toPlayer.y = 0;
            // toPlayer = toPlayer.normalized;
            // tank.turretAim = toPlayer;
        }
        else
        {
            RotateTowardsPlayer();
            readyToShoot = StraightShootingPath();            
        }

        if (readyToShoot)
        {
            shootingBurstCooldown -= Time.deltaTime;

            if (shootingBurstCooldown < 0)
            {                
                shootingCooldown -= Time.deltaTime;

                if (shootingCooldown < 0)
                {
                    switch (powerUp)
                    {
                        case PowerUp.none:                        
                            tank.Shoot();
                            break;
                        case PowerUp.rocket:
                            tank.Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.rocket), 0);
                            break;
                        case PowerUp.homingRocket:
                            tank.Shoot(PrefabManager.Prefab(PrefabManager.PrefabType.homingRocket), 0);
                            break;
                        case PowerUp.shield:
                            tank.Shoot();
                            break;
                    }         
                    
                    shootingBurstShotsFired++;
                    ResetShootingCooldown();
                }

                if (shootingBurstShotsFired >= liveShootingBurstCount)
                {
                    ResetShootingBurstCooldown();
                    RandomiseLiveShootingBurstCount();
                    shootingBurstShotsFired = 0;
                }                
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        float tolerance = 0.01f;
        Vector3 toPlayer = (target.transform.position - tank.transform.position).normalized;
        float rotateAmount = Time.deltaTime * tank.rotationSpeed;
        Vector3 rotationVector = new Vector3(0, rotateAmount, 0);

        Vector3 antiClockwiseRotation = Quaternion.Euler(-rotationVector) * tank.transform.forward;
        Vector3 clockwiseRotation = Quaternion.Euler(rotationVector) * tank.transform.forward;

        bool clockwise = (antiClockwiseRotation - toPlayer).magnitude > (clockwiseRotation - toPlayer).magnitude;

        Tank.TurningDirection turningDirection;

        if ((clockwise && (clockwiseRotation.normalized - toPlayer).magnitude > tolerance)
            || (!clockwise && (antiClockwiseRotation.normalized - toPlayer).magnitude > tolerance))
        {
            turningDirection = clockwise ? Tank.TurningDirection.clockwise : Tank.TurningDirection.anticlockwise;
            tank.TurnInDirection(turningDirection, 1);
        }
    }

    private Vector3 ChooseDirection(int resolution = 16)
    {
        List<float> distances = new List<float>();                

        for(int i = 0; i < resolution; i++)
        {
            float w = WalkableDistance(i * 360 / resolution);
            distances.Add(w);
        }

        int n = Random.Range(0, resolution / 2);

        while (n > 0)
        {
            n--;
            float m = distances.Max();
            distances[distances.IndexOf(m)] = 0;
        }

        float chosenDistance = distances.Max();

        int j = distances.IndexOf(chosenDistance);
        int angle = j * 360 / resolution;

        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
    }

    private Tank NearestPlayer()
    {
        List<Tank> playerTanks = new List<Tank>();
        PlayerTank[] playerTanks1 = FindObjectsOfType<PlayerTank>();

        foreach (PlayerTank playerTank1 in playerTanks1)
        {
            playerTanks.Add(playerTank1.GetComponent<Tank>());
        }           

        float d = Mathf.Infinity;

        if (playerTanks.Count == 0)
        {
            return null;
        }

        Tank nearestPlayer = playerTanks[0];

        foreach (Tank playerTank in playerTanks)
        {
            float d1 = (playerTank.transform.position - tank.transform.position).magnitude;
            if (d1 < d) { nearestPlayer = playerTank; d = d1; }
        }

        return nearestPlayer;
    }

    private bool StraightShootingPath()
    {
        Vector3 position = tank.turretMarker.transform.position;
        Vector3 direction = tank.transform.forward;
        return CheckShootingPath(position, direction);
    }

    private bool EstimateShootingPath()
    {
        Vector3 direction = (target.transform.position - tank.transform.position).normalized;
        return CheckShootingPath(transform.position, direction);
    }

    private bool CheckShootingPath(Vector3 position, Vector3 direction)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 projectedPosition = position;                   

        while (!Arena.Impassable(projectedPosition, 0))
        {
            projectedPosition += 0.1f * direction;

            if (Arena.TileX(projectedPosition)
                == Arena.TileX(target.transform.position)
                && Arena.TileY(projectedPosition)
                == Arena.TileY(target.transform.position))
            {
                return true;                
            }
        }

        return false;
    }    

    private float WalkableDistance(float angle)
    {        
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

        return WalkableDistance(direction);
    }

    private float WalkableDistance()
    {
        return WalkableDistance(transform.forward);
    }

    private float WalkableDistance(Vector3 direction)
    {
        Vector3 projectedPosition = transform.position;        

        float distance = 0;

        while (!Arena.Impassable(projectedPosition, 1) && distance < 100)
        {
            projectedPosition += 0.1f * direction;
            distance += 0.1f;
        }

        return distance;
    }
}
