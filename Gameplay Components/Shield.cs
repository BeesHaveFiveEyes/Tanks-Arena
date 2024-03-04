using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public Tank owner;

    private void Awake()
    {
        // Expand from centre of tank when activated
        Vector3 scale = transform.localScale;
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, scale, 0.5f).setEaseOutExpo();
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.GetComponent<Bullet>() != null)
        {
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            if (bullet.owner != owner || bullet.hasBounced)
            {
                other.gameObject.GetComponent<Bullet>().Die();
                owner.gameObject.GetComponent<PlayerTank>().RemovePowerUp();
                Die();
            }            
        }

        if (other.gameObject.GetComponent<Rocket>() != null)
        {
            Rocket rocket = other.gameObject.GetComponent<Rocket>();
            if (rocket.tank != owner)
            {
                other.gameObject.GetComponent<Rocket>().Die();
                owner.gameObject.GetComponent<PlayerTank>().RemovePowerUp();
                Die();
            }
        }        
    }

    public void Die()
    {
        // TODO
        AudioManager.Play("Shield Destroyed");

        void Destruct()
        {
            Destroy(gameObject);
        }

        LeanTween.scale(gameObject, Vector3.zero, 0.2f).setOnComplete(Destruct);
    }
}
