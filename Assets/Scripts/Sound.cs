using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    [Header("<================8")]
    public bool loop;

    [Header("<================8")]
    public bool randomisePitch;
    [Range(.1f, 3f)]
    public float minVal;
    [Range(.1f, 3f)]
    public float maxVal;

    [HideInInspector]
    public AudioSource source;
}
