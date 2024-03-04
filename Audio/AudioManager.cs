using UnityEngine.Audio;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    // A static reference to the current audio manager
	public static AudioManager instance;

    // The audio mixer group in use
	public AudioMixerGroup mixerGroup;

    // The music collection in the current scene, if any, storing the music tracks to loop through
    public MusicCollection currentMusicCollection;

    // An array storing all sounds used in the game
	public Sound[] sounds;

    // Is music enabled?
    public bool overrideMusicPreference = false;

    // The maximum number of sound effects that can be played simultaneously
    public int maxSimultaneousSoundEffects = 30;

    // An audio sources for music
    private AudioSource musicSource;

    // An array of the audio sources used for sound effects
    private List<AudioSource> soundEffectSources;

    // A record of the last known music volume
    private float lastMusicVolume;

    // A tracker for the current sound effect audio source
    // The tracker loops back around when it runs out
    private int currentSoundEffectSource;

    // A tracker for the current music clip being played
    // A tracker for the current music clip being played
    private int currentMusicIndex;

	void Awake()
	{
        // Create the audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        soundEffectSources = new List<AudioSource>();
        for (int i = 0; i < maxSimultaneousSoundEffects; i++)
        {
            soundEffectSources.Add(gameObject.AddComponent<AudioSource>());
        }                

        // Set the current instance
        instance = this;

        InitialiseMusic();
        
        if (currentMusicCollection!= null)
        {
            if (currentMusicCollection.playAutomatically)
            {
                ResumeMusic();
            }            
        }
	}

    private void InitialiseMusic()
    {
        currentMusicCollection = FindObjectOfType<MusicCollection>();
        if (currentMusicCollection != null)
        {
            Sound[] musicClips = currentMusicCollection.tracks;
            if (musicClips.Length > 0 && (Preferences.music.value || overrideMusicPreference))
            {            
                Sound musicSound = musicClips[UnityEngine.Random.Range(0, musicClips.Length)];

                // Set the properties of the source
                musicSource.clip = musicSound.clip;
                musicSource.loop = musicSound.looping;            
                musicSource.volume = musicSound.volume * (1f + UnityEngine.Random.Range(-musicSound.volumeVariance / 2f, musicSound.volumeVariance / 2f));
                musicSource.pitch = musicSound.pitch * (1f + UnityEngine.Random.Range(-musicSound.pitchVariance / 2f, musicSound.pitchVariance / 2f));

                instance.lastMusicVolume = instance.musicSource.volume;

                musicSource.Play(); 
                musicSource.Pause();
            }
        }        
    }

    public static void PauseMusic()
    {
        if (instance == null) 
        {
            Debug.LogError("No Audio Manager instance detected");
            return;
        }
        
        instance.lastMusicVolume = instance.musicSource.volume;

        LeanTween.value(instance.gameObject, instance.musicSource.volume, 0, 0.5f)
        .setOnUpdate((float t) => {
            instance.musicSource.volume = t;
        })
        .setOnComplete(()=>{instance.musicSource.Pause();});
    }

    public static void ResumeMusic()
    {
        if (instance == null) 
        {
            Debug.LogError("No Audio Manager instance detected");
            return;
        }
        
        instance.musicSource.UnPause();
        instance.musicSource.volume = instance.lastMusicVolume;
    }

    public static void Play(string soundName, float delay = 0)
    {
        if (instance == null) 
        {
            Debug.LogError("No Audio Manager instance detected");
            return;
        }

        instance.PlaySound(soundName, delay);        
    }

	private void PlaySound(string soundName, float delay = 0)
	{        
        Sound sound = Array.Find(sounds, item => item.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning("Sound '" + soundName + "' could not be found.");
            return;
        }

        if (Preferences.soundEffects.value || sound.soundType == Sound.SoundType.UI)
        {
            // Create a new source for this sound effect
            AudioSource audioSource;
                        
            audioSource = soundEffectSources[currentSoundEffectSource];
            currentSoundEffectSource = (currentSoundEffectSource + 1) % maxSimultaneousSoundEffects;         
                        
            // Set the properties of the source
            audioSource.clip = sound.clip;
            audioSource.loop = sound.looping;            
            audioSource.volume = sound.volume * (1f + UnityEngine.Random.Range(-sound.volumeVariance / 2f, sound.volumeVariance / 2f));
            audioSource.pitch = sound.pitch * (1f + UnityEngine.Random.Range(-sound.pitchVariance / 2f, sound.pitchVariance / 2f));
            audioSource.outputAudioMixerGroup = mixerGroup;            

            // Play the sound after the specified delay
            if (delay == 0)
            {
                audioSource.Play();                
            }
            else
            {
                StartCoroutine(Do());
            }

            IEnumerator Do()
            {
                yield return new WaitForSeconds(delay);
                audioSource.Play();
            }
        }        
    }
    
    public static void FadeOutAll(float duration = 0.5f)
    {     
        if (instance == null) 
        {
            Debug.LogError("No Audio Manager instance detected");
            return;
        }

        foreach (AudioSource source in instance.soundEffectSources)
        {      
            float originalVolume = source.volume;

            LeanTween.value(instance.gameObject, source.volume, 0, duration)
                .setEase(LeanTweenType.linear)
                .setOnUpdate((float val) =>
            {
                source.volume = val;
            })
            .setOnComplete(() => {
                source.volume = originalVolume;
                source.Pause();
            });
        }

        float originalMusicVolume = instance.musicSource.volume;

        LeanTween.value(instance.gameObject, instance.musicSource.volume, 0, duration)
                .setEase(LeanTweenType.linear)
                .setOnUpdate((float val) =>
            {
                instance.musicSource.volume = val;
            })
            .setOnComplete(() => {
                instance.musicSource.volume = originalMusicVolume;
                instance.musicSource.Pause();
            });
    }

}
