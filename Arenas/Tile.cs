using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile
{
    // All the possible objects that can appear on the surface of the tile
    public enum SurfaceObject
    {
        none,
        crate,
        redBarrel,
        powerUp,
        playerParts,        
        yellowParts,
        greenParts,
        orangeParts,
        rocketParts,
        navyParts,
        blackParts
    }    

    // Is the tile closed (i.e. is it a wall tile)?
    public bool closed;

    // Does the tile contain wiring
    public bool wired;

    // The object on the surface of the tile
    public SurfaceObject surfaceObject;

    // Is the tile impassable to players?
    public bool Impassable
    {
        get
        {
            if (closed)
            {
                return true;
            }
            else switch (surfaceObject)
            {                    
                case SurfaceObject.crate:
                case SurfaceObject.redBarrel:
                    return true;
                default:
                    return false;
            }                          
        }
    }

    // Is the tile impassable to bullets?
    public bool BulletImpassable
    {
        get
        {
            return closed;
        }
    }

    // Initialisers
    public Tile()
    {
        closed = false;
        wired = false;
        surfaceObject = SurfaceObject.none;
    }    

    public string DebugDescription()
    {
        if (closed)
        {
            return "X";
        }
        switch (surfaceObject)
        {
            case SurfaceObject.crate:
                return "c";
            case SurfaceObject.redBarrel:
                return "b";
            case SurfaceObject.playerParts:
            case SurfaceObject.yellowParts:
            case SurfaceObject.greenParts:
            case SurfaceObject.orangeParts:
            case SurfaceObject.navyParts:
            case SurfaceObject.rocketParts:
            case SurfaceObject.blackParts:            
                return "T";
            case SurfaceObject.none:
                return " ";
            default:
                return "?";
        }
    }
    
    public Tile Clone()
    {
        Tile output = new Tile();
        output.closed = closed;
        output.surfaceObject = surfaceObject;
        output.wired = wired;
        return output;
    }
}
