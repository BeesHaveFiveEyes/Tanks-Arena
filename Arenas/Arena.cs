using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// The Arena class is responsible for actually creating the game arenas in the scene.
// Arena.CreateArena(Level) creates the requisite gameobjects to generate the arena
// corresponding to the input level. This class also handles certain other tasks such
// as responding to explosions, tracking players and managing passable terrain.

public class Arena : MonoBehaviour
{
    // The current arena, if one has been created
    public static Arena currentArena;

    // A list of all the blocks making up the current scene
    public List<List<Block>> blockMatrix;

    // The current level represented in the arena
    public Level level;    

    // Lists of gameobjects currently present in the scene
    public static List<Tank> tanks = new List<Tank>();
    public static List<Tank> AITanks = new List<Tank>();
    public static List<PlayerTank> playerTanks = new List<PlayerTank>();
    public static List<Barrel> barrels = new List<Barrel>();
    public static List<Crate> crates = new List<Crate>();

    // --------------------
    // Instantiating Arenas
    // --------------------

    // Generate a new arena from a Level object
    public static void CreateArena(Level _level)
    {
        Debug.Log("Creating Arena from level '" + _level.levelName + "'");

        // Clone the input level
        Level level = _level.Clone();        

        // Destroy the current arena (if one exists)
        Destroy(currentArena);

        // Reset the gameobject lists
        ResetLists();
        
        // Create a 'root' gameobject with the Arena component
        GameObject arenaHook = new GameObject(level.levelName);
        Arena arena = arenaHook.AddComponent<Arena>();

        // Creating each of the blocks making up the arena's block matrix
        List<List<Block>> blockMatrix = new List<List<Block>>();
        for (int i = 0; i < level.Height; i++)
        {
            List<Block> row = new List<Block>();
            for (int j = 0; j < level.Width; j++)
            {
                // Name the block
                string blockName = "Block (" + i.ToString() + "," + j.ToString() + ")";

                // Position the block in the scene
                GameObject blockObject = new GameObject(blockName);
                blockObject.transform.parent = arenaHook.transform;
                blockObject.transform.position = new Vector3(i, 0, j);
                
                // Add the block component
                Block block = blockObject.AddComponent<Block>();

                // Apply information from the input level to the block
                block.tile = level.tileMatrix[i][j];
                block.level = level;
                block.arena = arena;
                block.x = i;
                block.y = j;

                // Add the block to the row
                row.Add(block);
            }

            blockMatrix.Add(row);
        }

        // Initialise each of the blocks
        foreach (List<Block> row in blockMatrix)
        {
            foreach (Block block in row)
            {
                block.Initialise();
            }
        }

        // Set the new arena as the static current arena
        
        if (currentArena != null)
        {
            Destroy(currentArena.gameObject);
        }
        
        arena.level = level;
        arena.blockMatrix = blockMatrix;
        currentArena = arena;

        // Activate any spawner elements to create the tanks
        
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        Spawner[] AISpawners = Array.FindAll(spawners, spawner =>  !spawner.player);
        Spawner[] PlayerSpawners = Array.FindAll(spawners, spawner =>  spawner.player);        
        
        for (int i = 0; i < AISpawners.Length; i++)
        {
            AISpawners[i].CreateTank();
        }
        
        Array.Sort(PlayerSpawners, delegate(Spawner s1, Spawner s2) { return s1.transform.position.z < s2.transform.position.z ? -1 : 1; });
        
        for (int i = 0; i < PlayerSpawners.Length; i++)
        {
            PlayerSpawners[i].CreateTank(i);
        }

        // Resume camera tracking

        CameraController.trackingPaused = false;        
    }

    // Create an empty arena
    // (e.g. when creating a level in the level editor)
    public static void CreateEmptyArena(int width = 29, int height = 29)
    {
        CreateArena(Level.EmptyLevel(height, width));
    }

    // ----------------
    // Arena Operations
    // ----------------

    // Remove all surface objects of the specified type from the current arena
    public static void RemoveSurfaceObjectsOfType(Tile.SurfaceObject type)
    {
        foreach (List<Block> row in currentArena.blockMatrix)
        {
            foreach (Block block in row)
            {
                if (block.tile.surfaceObject == type)
                {
                    block.tile.surfaceObject = Tile.SurfaceObject.none;
                    block.Reconfigure();
                }
            }
        }
    }

    // Manage the effects of an explosion at the specified position with the given radius 
    public static void CreateExplosion(Vector3 center, float radius = 4)
    {
        List<Tank> tanksCopy = new List<Tank>(tanks);
        List<Crate> cratesCopy = new List<Crate>(crates);
        List<Barrel> barrelsCopy = new List<Barrel>(barrels);

        foreach (Tank tank in tanksCopy)
        {
            if (tank != null)
            {
                if ((tank.transform.position - center).magnitude < radius)
                {
                    tank.Die();
                }
            }
        }

        foreach (Crate crate in cratesCopy)
        {
            if (crate != null)
            {
                if ((crate.transform.position - center).magnitude < radius)
                {
                    crate.Break();
                }
            }
        }

        foreach (Barrel barrel in barrelsCopy)
        {
            if (barrel != null)
            {
                if ((barrel.transform.position - center).magnitude < radius)
                {
                    barrel.Explode();                    
                }
            }
        }
    }

    // Reset all of the gameobject lists stored by this arena
    public static void ResetLists()
    {
        tanks = new List<Tank>();
        AITanks = new List<Tank>();
        playerTanks = new List<PlayerTank>();
        barrels = new List<Barrel>();
        crates = new List<Crate>();
    }

    // ----------------
    // Helper functions
    // ----------------

    // X distance into tile
    public static float InternalX(Vector3 position)
    {
        return (position.x + 0.5f) % 1;
    }

    // Y distance into tile
    public static float InternalZ(Vector3 position)
    {
        return (position.z + 0.5f) % 1;
    }

    // Tile x coordinate
    public static int TileX(Vector3 position)
    {
        return (int)(position.x - InternalX(position) + 0.5f);
    }

    // Tile y coordinate
    public static int TileY(Vector3 position)
    {
        return (int)(position.z - InternalZ(position) + 0.5f);
    }

    // Is a position in space impassable to tanks?
    public static bool Impassable(Vector3 position, float proximityTolerance)
    {
        int x = TileX(position);
        int y = TileY(position);
        float l = InternalX(position);
        float d = InternalZ(position);
        float r = 1 - l;
        float u = 1 - d;
        float p = proximityTolerance;

        List<List<Tile>> M = Arena.currentArena.level.tileMatrix;

        float Diag(float i, float j)
        {
            return Mathf.Sqrt(Mathf.Pow(i, 2) + Mathf.Pow(j, 2));
        }

        if (M[x][y].Impassable)
        {
            return true;
        }

        if (M[x + 1][y].Impassable && r < p)
        {
            return true;
        }

        if (M[x + 1][y + 1].Impassable && Diag(u, r) < p)
        {
            return true;
        }

        if (M[x][y + 1].Impassable && u < p)
        {
            return true;
        }

        if (M[x - 1][y + 1].Impassable && Diag(u, l) < p)
        {
            return true;
        }

        if (M[x - 1][y].Impassable && l < p)
        {
            return true;
        }

        if (M[x - 1][y - 1].Impassable && Diag(d, l) < p)
        {
            return true;
        }

        if (M[x][y - 1].Impassable && d < p)
        {
            return true;
        }

        if (M[x + 1][y - 1].Impassable && Diag(d, r) < p)
        {
            return true;
        }

        return false;
    }

    // Is a position in space impassable to bullets?
    public static bool BulletImpassable(Vector3 position, float proximityTolerance)
    {
        int x = TileX(position);
        int y = TileY(position);
        float l = InternalX(position);
        float d = InternalZ(position);
        float r = 1 - l;
        float u = 1 - d;
        float p = proximityTolerance;

        List<List<Tile>> M = Arena.currentArena.level.tileMatrix;

        float Diag(float i, float j)
        {
            return Mathf.Sqrt(Mathf.Pow(i, 2) + Mathf.Pow(j, 2));
        }

        if (M[x][y].BulletImpassable)
        {
            return true;
        }

        if (M[x + 1][y].BulletImpassable && r < p)
        {
            return true;
        }

        if (M[x + 1][y + 1].BulletImpassable && Diag(u, r) < p)
        {
            return true;
        }

        if (M[x][y + 1].BulletImpassable && u < p)
        {
            return true;
        }

        if (M[x - 1][y + 1].BulletImpassable && Diag(u, l) < p)
        {
            return true;
        }

        if (M[x - 1][y].BulletImpassable && l < p)
        {
            return true;
        }

        if (M[x - 1][y - 1].BulletImpassable && Diag(d, l) < p)
        {
            return true;
        }

        if (M[x][y - 1].BulletImpassable && d < p)
        {
            return true;
        }

        if (M[x + 1][y - 1].BulletImpassable && Diag(d, r) < p)
        {
            return true;
        }

        return false;
    }    
}
