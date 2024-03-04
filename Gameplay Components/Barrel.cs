using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    private void Awake()
    {
        Arena.barrels.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Bullet>() != null)
        {
            other.gameObject.GetComponent<Bullet>().Die();
            Destroy(other.gameObject);
            Arena.CreateExplosion (transform.position);
            Explode();
        }

        if (other.gameObject.GetComponent<Rocket>() != null)
        {
            other.gameObject.GetComponent<Rocket>().Die();
            Destroy(other.gameObject);
            Arena.CreateExplosion(transform.position);
            Explode();
        }
    }

    public void Explode()
    {
        Block block = transform.parent.GetComponent<Block>();
        block.tile.surfaceObject = Tile.SurfaceObject.none;
        block.Reconfigure();

        GameObject explosion = Instantiate(
            PrefabManager.Prefab(PrefabManager.PrefabType.explosion),
            transform.position,
            Quaternion.identity
        );

        explosion.transform.localScale *= 2;

        AudioManager.Play("Barrel Death");
        Arena.barrels.Remove(this);
        Destroy(gameObject);
    }
}

