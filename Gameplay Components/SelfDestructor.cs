using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    public float minimumLifetime = 3f;
    public float maximumLifetime = 6f;

    public float transitionTime = 0.5f;

    public bool scaleInX = true;
    public bool scaleInY = true;
    public bool scaleInZ = true;

    private float remainingLifetime = 100f;
    private bool destructionInProgress;

    private Vector3 finalScale {
        get {
            return new Vector3(scaleInX ? 0 : 1, scaleInY ? 0 : 1, scaleInZ ? 0 : 1);
        }
    }
    void Start()
    {
        remainingLifetime = Random.Range(minimumLifetime, maximumLifetime);
    }

    void Update()
    {
        if (remainingLifetime > 0)
        {
            remainingLifetime -= Time.deltaTime;            
        }
        else {
            Destruct();
        }
    }

    // Self destruct
    void Destruct()
    {               
        if (!destructionInProgress) {

            // Remove any physics components

            if (TryGetComponent(out Collider collider))
            {
                Destroy(collider);
            }
            
            if (TryGetComponent(out Rigidbody rigidbody))
            {
                Destroy(rigidbody);
            }

            // Scale away the object

            LeanTween.scale(gameObject, finalScale, transitionTime).setOnComplete( () =>
            {
                Destroy(gameObject);
            }).setEase(LeanTweenType.easeInOutExpo);
        }

        destructionInProgress = true;
    }
}
