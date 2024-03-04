using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    private bool alive = true;

    private void Awake()
    {
        Arena.crates.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.GetComponent<Bullet>() != null)
        {
            other.gameObject.GetComponent<Bullet>().Die();
            Destroy(other.gameObject);
            Break();
        }

        if (other.gameObject.GetComponent<Rocket>() != null)
        {
            other.gameObject.GetComponent<Rocket>().Die();
            Destroy(other.gameObject);
            Break();
        }
        
        // if (other.gameObject.GetComponent<Tank>() != null)
        // {
        //     Break();
        // }
    }

    public void Break()
    {    
        if (alive)
        {
            alive = false;    
            Block block = transform.parent.GetComponent<Block>();            
            block.tile.surfaceObject = Tile.SurfaceObject.none;
            block.Reconfigure();

            GameObject remains = Instantiate(
                PrefabManager.Prefab(PrefabManager.PrefabType.destructibleCrate),
                transform.position,
                transform.rotation
            );

            remains.transform.localScale = transform.localScale;
            remains.transform.parent = Arena.currentArena.transform;

            AudioManager.Play("Crate Destruction");

            Arena.crates.Remove(this);
            Destroy(gameObject);        
        }
    }
}
