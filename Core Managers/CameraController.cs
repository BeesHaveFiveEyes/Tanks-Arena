using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Serializable]
    public class Handle
    {
        public float trackingSpeed = 1;
        public float centerWeighting = 0.2f;
        public float fieldOfView = 50;
        public float cameraDistance = 35;
        public float elevation = 90;
        public float orbitalFrequency = 0.05f;
        public float orbitTime = 4;        
    }    

    [SerializeField] private Handle stationaryHandle;
    [SerializeField] private Handle defaultHandle;
    [SerializeField] private Handle tightHandle;
    [SerializeField] private Handle orbittingTankHandle;
    [SerializeField] private Handle orbittingArenaHandle;
    [SerializeField] private Handle dizzyHandle;

    public enum State
    {
        stationary,
        standard,
        tight,
        orbittingTanks,
        orbittingArena,
        dizzy
    }
    
    public GameObject anchor;

    public Vector3 arenaCenter;
    
    private Vector3 targetPosition;
    private float orbitTimer;
    private int orbitIndex;

    public static bool trackingPaused = false;

    [SerializeField] private State state;

    public void SetMode(State mode)
    {
        state = mode;

        switch (state)
        {
            case State.stationary:                
            case State.standard:                
            case State.tight:  
            case State.dizzy:              
                return;
            case State.orbittingTanks:
            case State.orbittingArena:                
                orbitTimer = 0;
                orbitIndex = UnityEngine.Random.Range(0, TankTargets().Count);
                return;
        }
    }

    private void Update()
    {
        if (!trackingPaused)
        {
            // Handle orbit timer to choose orbit target
            orbitTimer += Time.deltaTime;
            
            if (state == State.orbittingTanks)
            {
                // Update the orbit target, if timer is up
                if (orbitTimer > CurrentHandle.orbitTime)
                {
                    orbitTimer = 0;
                    orbitIndex++;
                    anchor.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f,360f), 0);
                }
            }

            // Handle position
            if (state == State.standard || state == State.tight)
            {                
                targetPosition = TanksMidpoint();
                Vector3 toTargetPosition = targetPosition - transform.position;
                anchor.transform.position += CurrentHandle.trackingSpeed * Time.deltaTime * toTargetPosition.magnitude * toTargetPosition;    
            }

            else if (state == State.orbittingArena || state == State.stationary || state == State.dizzy)
            {
                anchor.transform.position = arenaCenter;
            }   
            else
            {
                if (TankTargets().Count > 0)
                {
                    anchor.transform.position = TankTargets()[orbitIndex % TankTargets().Count].transform.position;
                }
            }    

            // Handle rotation (orbit if appropriate)
            if (state == State.orbittingTanks || state == State.orbittingArena || state == State.dizzy)
            {
                float angleDelta = 360 * CurrentHandle.orbitalFrequency * Time.deltaTime;
                anchor.transform.RotateAround(anchor.transform.position, Vector3.up, angleDelta);
            }
            else
            {
                anchor.transform.eulerAngles = new Vector3(0, -90, 0);
            }

            // Apply field of view
            Camera.main.fieldOfView = CurrentHandle.fieldOfView;

            // Apply camera distance
            Vector3 p = Camera.main.gameObject.transform.localPosition;
            p.y = CurrentHandle.cameraDistance;
            Camera.main.gameObject.transform.localPosition = p;

            // Apply camera elevation
            Vector3 angles = anchor.transform.eulerAngles;
            angles.x = -(90 - CurrentHandle.elevation);
            anchor.transform.eulerAngles = angles;
        }

    }

    private List<Tank> TankTargets()
    {
        if (state == State.tight)
        {
            // Target only player tanks
            return Arena.playerTanks.ConvertAll(playerTank => playerTank.tank).FindAll(tank => tank != null);
        }
        else
        {
            // Target all tanks
            return Arena.tanks.FindAll(tank => tank != null);
        }       
    }

    // private void HandleZoom()
    // {        
    //     List<Tank> targets = Targets();

    //     float minX = float.PositiveInfinity;
    //     float maxX = float.NegativeInfinity;
    //     float minY = float.PositiveInfinity;
    //     float maxY = float.NegativeInfinity;

    //     foreach (Tank target in targets)
    //     {
    //         // Vector2 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);
    //         Vector2 screenPoint = Camera.main.WorldToViewportPoint(target.transform.position);
            
    //         if (screenPoint.x < minX)
    //         {
    //             minX = screenPoint.x;
    //         }
    //         if (screenPoint.x > maxX)
    //         {
    //             maxX = screenPoint.x;
    //         }
    //         if (screenPoint.y < minY)
    //         {
    //             minY = screenPoint.y;
    //         }
    //         if (screenPoint.y > maxY)
    //         {
    //             maxY = screenPoint.y;
    //         }

    //         // Camera.main.fieldOfView = 

    //         Debug.Log("x: " + (maxX - minX).ToString());
    //         Debug.Log("y: " + (maxY - minY).ToString());

    //         Debug.Log(Screen.height);
    //         Debug.Log(Screen.width);
    //     }
    // }

    private Vector3 TanksMidpoint()
    {        
        List<Tank> targets = TankTargets();

        if (targets.Count == 0)
        {
            return arenaCenter;    
        }

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;

        foreach (Tank target in targets)
        {            
            Vector3 position = target.transform.position;
            
            if (position.x < minX)
            {
                minX = position.x;
            }
            if (position.x > maxX)
            {
                maxX = position.x;
            }
            if (position.z < minZ)
            {
                minZ = position.z;
            }
            if (position.z > maxZ)
            {
                maxZ = position.z;
            }
        }  

        Vector3 midpoint = 0.5f * new Vector3(minX + maxX, 0 ,minZ + maxZ);
        return CurrentHandle.centerWeighting * arenaCenter + (1 - CurrentHandle.centerWeighting) * midpoint;
    }

    // public void SpinCamera(float speed)
    // {
    //     angularSpeed = speed;
    // }

    // public void StepCamera(float speed)
    // {
    //     if (GameManager.context == GameManager.Context.multiplayer)
    //     {
    //         transform.localScale = (1 / zoom / multiplayerZoom) * new Vector3(1, 1, 1);
    //     }
    //     else
    //     {
    //         
    //     }        
    // }


    // -----------------
    // Helper Properties
    // -----------------

    private Handle CurrentHandle
    {
        get
        {
            switch (state)
            {
                case State.stationary:
                    return stationaryHandle;
                case State.standard:
                    return defaultHandle;
                case State.tight:
                    return tightHandle;
                case State.orbittingTanks:
                    return orbittingTankHandle;
                case State.orbittingArena:
                    return orbittingArenaHandle;
                default:
                    return stationaryHandle;
            }
        }
    }
}
