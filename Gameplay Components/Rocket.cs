using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{        
    [HideInInspector]
    public Tank tank;
    public Tank enemyTank;

    [HideInInspector]
    public float rocketSpeed;
    public bool homing;

    private float speedMultiplier;

    private void Awake()
    {
        if (homing)
        {
            AudioManager.Play("Rocket Fired");
        }        
        else
        {
            AudioManager.Play("Homing Rocket Fired");
        }

        speedMultiplier = homing ? 1 : 2;

        if (homing)
        {
            StartCoroutine(FindTarget());
            homing = false;
        }                
    }

    private IEnumerator FindTarget()
    {
        yield return new WaitForSeconds(0.01f);

        homing = true;

        PlayerTank[] tanks = FindObjectsOfType<PlayerTank>();

        if (tanks.Length > 1)
        {
            if (tanks[0].tank == tank)
            {
                Debug.Log("tanks[0] == tank, so enemyTank = tanks[1].tank");
                enemyTank = tanks[1].tank;
            }
            else
            {
                Debug.Log("tanks[0] != tank, so enemyTank = tanks[0].tank");
                enemyTank = tanks[0].tank;
            }
        }                
    }

    void Update()
    {
        //transform.Rotate(Vector3.forward, 100 * Time.deltaTime);

        if (enemyTank != null && homing)
        {
            Quaternion temp = transform.rotation;            
            transform.LookAt(enemyTank.transform.position);
            Quaternion target = transform.rotation;
            transform.rotation = temp;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, Time.deltaTime * 150);
        }        

        Vector3 newPosition = transform.position + rocketSpeed * speedMultiplier * Time.deltaTime * transform.forward;

        if (Arena.BulletImpassable(newPosition, 0))
        {
            Die();
        }

        if(!GameManager.current.paused)
        {
            transform.position = newPosition;
        }        
    }

    public void Die()
    {        
        GameObject explosion = Instantiate(
            PrefabManager.Prefab(PrefabManager.PrefabType.explosion),
            transform.position,
            Quaternion.identity
        );
        explosion.transform.localScale *= 2;
        Arena.CreateExplosion(transform.position);

        AudioManager.Play("Rocket Death");

        Destroy(gameObject);
    }
}
