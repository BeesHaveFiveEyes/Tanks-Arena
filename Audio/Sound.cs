using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {

	public enum SoundType {
		soundEffect,
		music,
		UI
	}

	// A unique name for the sound
	public string name;

	// The audio clip file for the sound
	public AudioClip clip;

	// The volume of the sound
	[Range(0f, 1f)] public float volume = .75f;

	// The ammount of random volume variation
	[Range(0f, 1f)]public float volumeVariance = .1f;

	// Pitch adjustment
	[Range(.1f, 3f)] public float pitch = 1f;
	
	// The ammount of random pitch variation
	[Range(0f, 1f)] public float pitchVariance = .1f;

	// Should the sound loop?
	public bool looping;

	// The type of sound
	public SoundType soundType = SoundType.soundEffect;
	
	// Is the sound enabled in the current scene?
	public bool enabled = true;

}
