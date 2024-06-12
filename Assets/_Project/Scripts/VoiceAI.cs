using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceAI_001", menuName = "SO/VoiceAI", order = 1)]
public class VoiceAI : ScriptableObject
{
    public List<VoiceData> _voices;

    public VoiceData GetFirstStart() => _voices.First(x => x.voiceType == VoiceType.START);
    public VoiceData GetFirstEnd() => _voices.First(x => x.voiceType == VoiceType.END);
    public VoiceData GetHintVoiceByIndex(int hintIndex) => _voices.Where(x => x.voiceType == VoiceType.HINT).ToList().ElementAt(hintIndex);

    public List<VoiceData> GetAllHintVoices() => _voices.Where(x => x.voiceType == VoiceType.HINT).ToList();
    public List<VoiceData> GetAllStartVoices() => _voices.Where(x => x.voiceType == VoiceType.START).ToList();
    public List<VoiceData> GetAllEndVoices() => _voices.Where(x => x.voiceType == VoiceType.END).ToList();
}

[Serializable]
public struct VoiceData
{
    public string text;
    public VoiceType voiceType;
    public AudioClip audio;
    public float delayedTime;
    public int numberOfRepeat;
}

public enum VoiceType
{
    START,
    END,
    HINT
}
