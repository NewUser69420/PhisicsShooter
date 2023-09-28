using System;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    public Transform TrackedObject;

    public bool isActive;

    private AudioSource Audio;

    private void Start()
    {
        Audio = GetComponent<AudioSource>();
        PlaySound();
    }

    private void Update()
    {
        if (!isActive) return;
        
        if(TrackedObject == null)
        {
            transform.position = transform.position;
            Invoke(nameof(Destroy), 1f);
            return;
        }
        else
        {
            transform.position = TrackedObject.position;
        }
    }

    public void PlaySound()
    {
        Audio.Play();
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
