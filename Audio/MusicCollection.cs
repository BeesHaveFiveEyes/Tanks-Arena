using UnityEngine.Audio;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public class MusicCollection : MonoBehaviour
{
    // Should the music play automatically?
    public bool playAutomatically = false;
    
    // An array storing all sounds used in the game
	public Sound[] tracks;
}
