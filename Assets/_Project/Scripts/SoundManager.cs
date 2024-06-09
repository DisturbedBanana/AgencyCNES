using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public void PlaySound(GameObject gameObject, AudioClip clip, float volume)
    {
        AudioSource audioSource = gameObject.TryGetComponent(out AudioSource source) ? source : gameObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;
    }

    public void StopSound(GameObject gameObject)
    {
        if(gameObject.TryGetComponent(out AudioSource audioSource))
        {
            audioSource.Stop();
        }
    }
    public void ChangeAudioVolume(GameObject gameObject, float volume)
    {
        if(gameObject.TryGetComponent(out AudioSource audioSource))
        {
            audioSource.volume = volume;
        }
    }
}
