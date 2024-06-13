using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ValveManager : NetworkBehaviour, IGameState
{
    public static ValveManager instance;

    [SerializeField] private ValvePuzzlePart[] _valvePuzzleParts;
    bool _isSolved = false;
    
    public bool IsSolved { get { return _isSolved; } set { _isSolved = value; } }

    [Header("Voices")]
    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    private NetworkVariable<int> _currentHintIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnStateComplete;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void StartState()
    {
        OnStateStart?.Invoke(); 
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
    }

    public void CheckValves()
    {
        foreach (ValvePuzzlePart valvePuzzlePart in _valvePuzzleParts)
        {
            if (!valvePuzzlePart.IsSolved)
            {
                _isSolved = false;
                return;
            }
        }

        _isSolved = true;
        OnStateCompleteClientRpc();
        GameState.Instance.ChangeState(GameState.GAMESTATES.FUSES);
    }

    [ClientRpc]
    public void OnStateCompleteClientRpc()
    {
        OnStateComplete?.Invoke();
        StopCoroutine(StartHintCountdown());
    }

    public IEnumerator StartHintCountdown()
    {
        if (_voicesHint.Count == 0)
            yield break;

        for (int i = 0; i < _voicesHint[_currentHintIndex.Value].numberOfRepeat + 1; i++)
        {
            float waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
            int waitingHintIndex = _currentHintIndex.Value;

            while (waitBeforeHint > 0)
            {
                if (waitingHintIndex != _currentHintIndex.Value)
                {
                    if (_currentHintIndex.Value > _voicesHint.Count - 1) yield break;
                    waitingHintIndex = _currentHintIndex.Value;
                    waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
                }

                waitBeforeHint -= Time.deltaTime;
                yield return null;
            }
            SoundManager.Instance.PlaySound(gameObject, _voicesHint[_currentHintIndex.Value].audio);
        }


        if (_currentHintIndex.Value < _voicesHint.Count - 1)
        {
            ChangeHintIndexServerRpc(_currentHintIndex.Value + 1);
            StartCoroutine(StartHintCountdown());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHintIndexServerRpc(int value)
    {
        _currentHintIndex.Value = value;
    }
}
