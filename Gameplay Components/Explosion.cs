using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    void Awake()
    {
        // Play an explosion sound when instantiated
        AudioManager.Play("Explosion");
    }
}
