using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{        
    // The tank that fired the bullet
    [HideInInspector]
    public Tank owner;

    // The remaining number of times the bullet can bounce
    public int bounces;

    // Has the bullet already bounced?
    public bool hasBounced = false;

    // The speed of the bullet
    public float bulletSpeed;
    
    public int id;

    // The direction of travel of the bullet
    private Vector3 direction;

    private void Awake()
    {
        AudioManager.Play("Bullet Fired");
        direction = transform.forward;        
    }

    void Update()
    {        
        Propagate();
    }

    // Continue the trajectory of the bullet, handling bounces
    private void Propagate()
    {
        // Calculate the proposed new position
        Vector3 newPosition = transform.position + bulletSpeed * Time.deltaTime * direction;

        // Handle arena collisions
        if (Arena.BulletImpassable(newPosition, 0))
        {
            // TODO
            // AudioManager.instance.Play("Collision");

            // Die if no bounces remain
            if (bounces <= 0)
            {   
                AudioManager.Play("Bullet Death");             
                Die();
            }

            // Otherwise, handle bouncing
            else
            {
                AudioManager.Play("Bullet Bounce");

                bounces--;
                hasBounced = true;                

                if (Arena.TileX(newPosition) != Arena.TileX(transform.position))
                {
                    // Horizontal bounce
                    direction.x = -direction.x;                    
                }
                else
                {
                    // Vertical bounce
                    direction.z = -direction.z;
                }
            }
        }

        // Apply new position
        else if (!GameManager.current.paused)
        {
            transform.position = newPosition;
        } 
    }

    // Handle the death of the bullet
    public void Die()
    {
        Destroy(gameObject);
    }
}
