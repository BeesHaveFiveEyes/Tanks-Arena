using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Tile;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager instance;
    private static bool initialised;

    public enum PrefabType
    {
        bullet,
        explosion,
        scorching,
        destructibleCrate,
        rocket,
        homingRocket,
        shield,
        gridMarker,
        crate,
        redBarrel,
        powerUp,
        playerParts,
        yellowParts,
        greenParts,
        orangeParts,
        rocketParts,
        navyParts,
        blackParts,
        powerupPopUp,
        powerUpParticles
    }

    public enum PrefabArrayType
    {
        wall
    }

    [System.Serializable]
    public class PrefabObject
    {
        public PrefabType prefabType;
        public GameObject prefab;
    }

    [System.Serializable]
    public class PrefabArray
    {
        public PrefabArrayType prefabType;
        public GameObject[] prefabs;
    }

    [SerializeField]
    private PrefabObject[] simplePrefabs;

    [SerializeField]
    private PrefabArray[] prefabArrays;

    [SerializeField]
    private List<GameObject> basePrefabsAssignable;

    public static List<GameObject> basePrefabs
    {
        get
        {
            if (!initialised) Initialise();
            return instance.basePrefabsAssignable;
        }
    }

    // Access a simple prefab for instantiaion in other scripts
    public static GameObject Prefab(PrefabType type)
    {
        if (!PrefabManager.initialised) Initialise();        

        foreach (PrefabObject prefabObject in instance.simplePrefabs)
        {
            if (prefabObject.prefabType == type)
            {
                return prefabObject.prefab;
            }
        }

        Debug.LogError("PrefabManager could not find this PrefabType: " + type.ToString());
        return null;
    }

    // Access a prefab from an array for instantiaion in other scripts
    public static GameObject Prefab(PrefabArrayType type, int index)
    {
        if (!PrefabManager.initialised) Initialise();

        foreach (PrefabArray prefabArray in instance.prefabArrays)
        {
            if (prefabArray.prefabType == type)
            {
                return prefabArray.prefabs[index];
            }
        }

        Debug.LogError("PrefabManager could not find this PrefabArrayType: " + type.ToString());
        return null;
    }

    // Return the surface prefab from a tile type
    public static GameObject SurfacePrefab(Tile tile)
    {
        switch (tile.surfaceObject)
        {                           
            case Tile.SurfaceObject.crate:
                return Prefab(PrefabType.crate);

            case Tile.SurfaceObject.redBarrel:
                return Prefab(PrefabType.redBarrel);

            case Tile.SurfaceObject.powerUp:
                return Prefab(PrefabType.powerUp);

            case Tile.SurfaceObject.playerParts:
                return Prefab(PrefabType.playerParts);

            case Tile.SurfaceObject.yellowParts:
                return Prefab(PrefabType.yellowParts);

            case Tile.SurfaceObject.greenParts:
                return Prefab(PrefabType.greenParts);

            case Tile.SurfaceObject.orangeParts:
                return Prefab(PrefabType.orangeParts);

            case Tile.SurfaceObject.rocketParts:
                return Prefab(PrefabType.rocketParts);

            case Tile.SurfaceObject.navyParts:
                return Prefab(PrefabType.navyParts);

            case Tile.SurfaceObject.blackParts:
                return Prefab(PrefabType.blackParts);

            default:
                return null;
        }
    }

    private void Awake()
    {
        Initialise();
    }

    private static void Initialise()
    {
        instance = FindObjectOfType<PrefabManager>();
        if (instance == null)
        {
            Debug.LogError("Prefab Manager not found.");
        }
        initialised = true;
    }
}
