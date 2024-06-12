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
    public void PlaySound(GameObject gameObject, AudioClip clip, float volume = 1)
    {
        AudioSource audioSource = gameObject.TryGetComponent(out AudioSource source) ? source : gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
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
    public void PlayVoices(GameObject go, List<VoiceData> voices)
    {
        if(voices.Count > 0)
            StartCoroutine(PlayVoicesRoutine(go, voices));
    }
    public IEnumerator PlayVoicesRoutine(GameObject go, List<VoiceData> voices)
    {
        for (int i = 0; i < voices.Count; i++)
        {
            yield return new WaitForSeconds(voices[i].delayedTime);
            PlaySound(go, voices[i].audio);
            yield return new WaitForSeconds(voices[i].audio.length);

        }
    }
}
