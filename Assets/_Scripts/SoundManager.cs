using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource effectsSource, musicSource;

    public Vector2 pitchRange = Vector2.zero;       //the range we will randomly change pitch

    public static SoundManager SharedInstance;


    private void Awake(){
        if (SharedInstance!=null){
            Destroy(gameObject);
        }else{
            SharedInstance = this;
        }
        //this makes that we can take the objects from 1 level to another without being destroyed
        DontDestroyOnLoad(gameObject);
    }

    //clip is audio
    public void PlaySound(AudioClip clip){
        effectsSource.pitch = 1;    //reset pitch each time
        effectsSource.Stop();       //to stop the old sound that could be playing before this code
        effectsSource.clip = clip;
        effectsSource.Play();
    }
    
    //In the world we play music
    public void PlayMusic(AudioClip clip){
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    //RANDOM PLAYED MUSIC
    //params is a reference
    public void RandomSoundEffect(params AudioClip[] clips){
        //if we are not globally using imported libraries, we can write it like this> UnityEngine.Random...etc
        int index = UnityEngine.Random.Range(0, clips.Length);
        float pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);

        effectsSource.pitch = pitch;
        PlaySound(clips[index]);
    }
}