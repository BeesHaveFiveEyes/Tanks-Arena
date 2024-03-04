using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// A Block object corresponds to one square of an active Arena.

public class Block : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // The level to behind the arena to which this block belongs         
    public Level level;

    // The level tile on which this block is based
    public Tile tile;

    // The arena to which this block belongs
    public Arena arena;

    // The gameobject respresenting the ground or wall element associated with this block
    [SerializeField] private GameObject baseObject;

    // A gameobject representing any object on the surface of this block, such as a crate of spawn point
    [SerializeField] private GameObject surfaceObject;

    // A visual marker attached to this block that is enabled in the arena editor
    public GridMarker gridMarker;

    // The coordinates of the block
    public int x;
    public int y;

    // Is the block on the edge of the map (and so locked)?
    public bool Edge
    {
        get
        {
            return x == 0 || x + 1 == level.Height || y == 0 || y + 1 == level.Width;
        }        
    }

    // Mouse detection
    [SerializeField] private bool mouseOver = false;
    [SerializeField] private bool mouseEntered = false;

    // The properties of the block before the most recent edit
    private bool wasEditing;    
    private bool previouslyClosed;
    private Tile.SurfaceObject previousSurfaceObjectType;

    // Perform actions to initialise the block
    public void Initialise(bool animated = false)
    {        
        ConfigureBaseObject(animated);
        ConfigureSurfaceObject();        
        wasEditing = Preferences.Editing;
        previouslyClosed = tile.closed;
        previousSurfaceObjectType = tile.surfaceObject;
    }

    // Update the visible properties of the block, if neccessary
    public void Reconfigure(bool animated = false)
    {
        ReconfigureEditing();

        if (tile.closed != previouslyClosed)
        {
            ConfigureBaseObject(animated);
            ReconfigureNeighbourBases();
        }

        if (tile.surfaceObject != previousSurfaceObjectType)
        {
            ConfigureSurfaceObject();            
        }

        previouslyClosed = tile.closed;
        previousSurfaceObjectType = tile.surfaceObject;

        RefreshGridMarker();
    }

    // Update the base objects of the neighbours of this tile
    public void ReconfigureNeighbourBases()
    {
        int n = level.Height;
        int m = level.Width;

        if (x > 0)
        {
            arena.blockMatrix[x - 1][y].ConfigureBaseObject();

            if (y > 0)
            {
                arena.blockMatrix[x - 1][y - 1].ConfigureBaseObject();
            }
            if (y + 1 < n)
            {
                arena.blockMatrix[x - 1][y + 1].ConfigureBaseObject();
            }
        }
        if (x + 1 < n)
        {
            arena.blockMatrix[x + 1][y].ConfigureBaseObject();

            if (y > 0)
            {
                arena.blockMatrix[x + 1][y - 1].ConfigureBaseObject();
            }
            if (y + 1 < n)
            {
                arena.blockMatrix[x + 1][y + 1].ConfigureBaseObject();
            }
        }
        if (y > 0)
        {
            arena.blockMatrix[x][y - 1].ConfigureBaseObject();
        }
        if (y + 1 < m)
        {
            arena.blockMatrix[x][y + 1].ConfigureBaseObject();
        }
    }

    // Update the block depending on whether level is being edited or not
    public void ReconfigureEditing()
    {
        if (wasEditing != Preferences.Editing)
        {
            Initialise();
        }        
    }

    // Instantiate the surface object for this block
    private void ConfigureSurfaceObject()
    {
        if (tile.surfaceObject == Tile.SurfaceObject.none)
        {            
            Destroy(surfaceObject);
            surfaceObject = null;
        }
        else
        {
            Destroy(surfaceObject);            
            surfaceObject = Instantiate(PrefabManager.SurfacePrefab(tile));            
            surfaceObject.transform.parent = transform;
            surfaceObject.transform.localPosition = Vector3.zero;
            surfaceObject.transform.rotation = Quaternion.Euler(-90, 0, -90);
            surfaceObject.transform.localScale *= 0.5f;
        }
    }    

    // Recalculate the base (wall or floor) object for the block
    private void ConfigureBaseObject(bool animated = false)
    {        
        int i = 0;
        int r = 0;

        if (tile.closed)
        {            
            bool[] code = new bool[8];
            bool[] closures = new bool[8];

            if (x == 0 || y == 0)
            {
                closures[0] = true;
            }
            else
            {
                closures[0] = level.tileMatrix[x - 1][y - 1].closed;
            }

            if (x == 0)
            {
                closures[1] = true;
            }
            else
            {
                closures[1] = level.tileMatrix[x - 1][y].closed;
            }

            if (x == 0 || y + 1 == level.Width)
            {
                closures[2] = true;
            }
            else
            {
                closures[2] = level.tileMatrix[x - 1][y + 1].closed;
            }

            if (y + 1 == level.Width)
            {
                closures[3] = true;
            }
            else
            {
                closures[3] = level.tileMatrix[x][y + 1].closed;
            }

            if (x + 1 == level.Height || y + 1 == level.Width)
            {
                closures[4] = true;
            }
            else
            {
                closures[4] = level.tileMatrix[x + 1][y + 1].closed;
            }

            if (x + 1 == level.Height)
            {
                closures[5] = true;
            }
            else
            {
                closures[5] = level.tileMatrix[x + 1][y].closed;
            }

            if (x + 1 == level.Height || y == 0)
            {
                closures[6] = true;
            }
            else
            {
                closures[6] = level.tileMatrix[x + 1][y - 1].closed;
            }

            if (y == 0)
            {
                closures[7] = true;
            }
            else
            {
                closures[7] = level.tileMatrix[x][y - 1].closed;
            }

            code[0] = !closures[7];
            code[2] = !closures[1];
            code[4] = !closures[3];
            code[6] = !closures[5];

            code[1] = closures[7]
                && closures[1]
                && !closures[0];

            code[3] = closures[1]
                && closures[3]
                && !closures[2];

            code[5] = closures[3]
                && closures[5]
                && !closures[4];

            code[7] = closures[5]
                && closures[7]
                && !closures[6];

            string codeString = "";

            for (int j = 0; j < 8; j++)
            {
                if (j % 2 == 0)
                {
                    codeString += code[j] ? "X" : "O";
                }
                else
                {
                    codeString += code[j] ? "c" : "o";
                }
            }

            for (int j = 0; j < 4; j++)
            {
                string rotatedString = codeString.Substring(2 * j, 8 - 2 * j) + codeString.Substring(0, 2 * j);

                foreach (GameObject prefab in PrefabManager.basePrefabs)
                {
                    if (prefab.name == rotatedString)
                    {
                        i = PrefabManager.basePrefabs.IndexOf(prefab);
                        r = j;
                    }
                }
            }

            if (i == 0)
            {
                Debug.LogWarning("Base element '" + codeString + "' not found.");
                return;
            }
        }

        void Spawn()
        {
            if (baseObject != null)
            {
                Destroy(baseObject);
            }

            baseObject = Instantiate(PrefabManager.basePrefabs[i]);
            baseObject.transform.parent = transform;
            baseObject.transform.localPosition = Vector3.zero;
            baseObject.transform.rotation = Quaternion.Euler(-90, 180 + 90 * r, -90);
            baseObject.transform.localScale *= 0.5f;
        }

        if (tile.closed && animated)
        {
            Spawn();
            Vector3 scale = baseObject.transform.localScale;
            scale.z = 0;
            baseObject.transform.localScale = scale;
            LeanTween.scaleZ(baseObject, 0.5f, 1)
                .setEaseOutElastic();
        }

        else
        {
            Spawn();
        }

        if (Preferences.Editing)
        {
            RemoveCollidersForEditing();
            if (gridMarker == null)
            {
                gridMarker = Instantiate(PrefabManager.Prefab(PrefabManager.PrefabType.gridMarker), transform).GetComponent<GridMarker>();
                gridMarker.SetHighlight(false);
                RefreshGridMarker();
            }
        }
        else {
            if (gridMarker != null)
            {
                Destroy(gridMarker.gameObject);
            }
        }
        
    }

    // Configure colliders for the arena editor
    private void RemoveCollidersForEditing()
    {
        Collider[] colliders = GetComponents<Collider>();
        Collider[] childrenColliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            Destroy(collider);
        }

        foreach (Collider collider in childrenColliders)
        {
            Destroy(collider);
        }

        BoxCollider c = gameObject.AddComponent<BoxCollider>();

        Vector3 newSize = c.size;
        newSize.y *= 0.01f;

        c.size = newSize;

        c.isTrigger = true;
    }

    // Detect and handle pointer entrance
    public void OnPointerEnter(PointerEventData eventData)
    {        
        mouseOver = true;
        if (gridMarker != null)
        {
            RefreshGridMarker();
            gridMarker.SetHighlight(true);
        }
    }

    // Detect and handle pointer exit
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        mouseEntered = false;
        if (gridMarker != null)
        {
            RefreshGridMarker();
            gridMarker.SetHighlight(false);
        }
    }    

    // Enable the grid marker if in the arena editor mode, otherwise disable it
    public void RefreshGridMarker()
    {
        if (gridMarker != null)
        {
            gridMarker.gameObject.SetActive(surfaceObject == null && !tile.closed);
        }       
        if (LevelEditor.current != null)
        {
            if (LevelEditor.current.currentTool != LevelEditor.LevelEditorTool.none)
            {
                gridMarker.SetHighlight(false);
            }        
        }         
    }

    private void Update()
    {
        //Handle 'on click' and 'on drag' events for this block via the LevelEditor
        
        if (mouseOver & !mouseEntered)
        {
            mouseEntered = true;

            if (!Edge && Preferences.Editing && Mouse.current.leftButton.isPressed)
            {                                
                LevelEditor.current.OnDrag(this);
            }
        }
        
        if (mouseOver && !Edge && Preferences.Editing && Mouse.current.leftButton.wasPressedThisFrame)
        {                        
            LevelEditor.current.OnClick(this);
        }
    }
}
