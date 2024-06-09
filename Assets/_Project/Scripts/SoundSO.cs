using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SFX_001", menuName = "SO/New SFX", order = 0)]
public class SoundSO : ScriptableObject
{
    public enum SoundType
    {
        SFX = 0, 
        MUSIC
    }

    [SerializeField] private SoundType _soundType;
    [SerializeField] private AudioClip _audioClip;

    public SoundType TypeOfSound { get => _soundType; set => _soundType = value; }
    public AudioClip AudioClip { get => _audioClip; set => _audioClip = value; }

}
