using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

public class Dodge : NetworkBehaviour, IGameState, IVoiceAI
{
    [Header("Levers")]
    [SerializeField] private XRLever _leverFusee;
    public NetworkVariable<bool> _dodgeLeverFuseeIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private XRLever _leverMissionControl;
    public NetworkVariable<bool> _dodgeLeverMissionControlIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
        StartCoroutine(StartHintCountdown());
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
                    if(_currentHintIndex.Value > _voicesHint.Count - 1) yield break;
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


    public void LeverActivated(int playerNumber)
    {
        ChangeLeverValueServerRpc(playerNumber, true);
    }
    public void LeverDeactivated(int playerNumber)
    {
        var lever = playerNumber == 0 ? _leverFusee : _leverMissionControl;
        lever.value = false;
        ChangeLeverValueServerRpc(playerNumber, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeLeverValueServerRpc(int playerNumber, bool value)
    {
        GetPlayerLever(playerNumber).Value = value;
        Debug.Log($"Player {playerNumber} lever is activated : {value}");
        if (GameState.Instance.CurrentGameState == GameState.GAMESTATES.DODGE)
        {
            if (!AreBothLeverActivated())
                return;

            OnStateCompleteClientRpc();
            GameState.Instance.ChangeState(GameState.GAMESTATES.WIN);
        }
    }


    [Rpc(SendTo.Everyone)]
    public void OnStateCompleteClientRpc()
    {
        OnStateComplete?.Invoke();
        ChangeHintIndexServerRpc(_currentHintIndex.Value + 1);
        StopCoroutine(StartHintCountdown());
    }

    private bool AreBothLeverActivated() => _dodgeLeverFuseeIsActivated.Value && _dodgeLeverMissionControlIsActivated.Value;
    private NetworkVariable<bool> GetPlayerLever(int playerNumber) => playerNumber == 0 ? _dodgeLeverFuseeIsActivated : _dodgeLeverMissionControlIsActivated;
}
