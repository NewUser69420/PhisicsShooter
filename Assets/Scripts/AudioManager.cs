using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManger : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManger instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Update()
    {
        foreach (Sound s in sounds)
        {
            if (s.randomisePitch)
            {
                float randomNum = UnityEngine.Random.Range(s.minVal, s.maxVal);
                s.source.pitch = randomNum;
            }
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("sound: " + name + "not found!");
            return;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("sound: " + name + "not found!");
            return;
        }
        s.source.Stop();
    }
}
