using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

public class Separation : NetworkBehaviour, IGameState
{
    [Header("Levers")]
    [SerializeField] private XRLever _leverFusee;
    private NetworkVariable<bool> _leverFuseeIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private XRLever _leverMissionControl;
    private NetworkVariable<bool> _leverMissionControlIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Voices")]
    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    private NetworkVariable<int> _currentHintIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnStateComplete;

    public void StartState()
    {
        OnStateStart?.Invoke();
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
    }
    public void LeverActivated(int playerNumber)
    {
        ChangeLeverValueServerRpc(playerNumber, true);
    }
    public void LeverDeactivated(int playerNumber)
    {
        ChangeLeverValueServerRpc(playerNumber, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeLeverValueServerRpc(int playerNumber, bool value)
    {
        GetPlayerLever(playerNumber).Value = value;

        if (GameState.Instance.CurrentGameState == GameState.GAMESTATES.SEPARATION)
        {
            CheckSeparationLeverServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    public void CheckSeparationLeverServerRpc(ulong clientID)
    {
        if (!AreBothLeverActivated())
            return;

        OnStateCompleteClientRpc();

        GameState.Instance.ChangeState(GameState.GAMESTATES.FREQUENCY);
    }

    private bool AreBothLeverActivated() => _leverFuseeIsActivated.Value && _leverMissionControlIsActivated.Value;
    private NetworkVariable<bool> GetPlayerLever(int playerNumber) => playerNumber == 0 ? _leverFuseeIsActivated : _leverMissionControlIsActivated;


    [Rpc(SendTo.Everyone)]
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
                    if (_currentHintIndex.Value > _voicesHint.Count - 1)
                    {
                        yield break;
                    }
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
