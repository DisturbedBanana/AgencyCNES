using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FrequenciesCheck : NetworkBehaviour, IGameState
{
    [SerializeField] private Frequency _frequencyLauncher;
    [SerializeField] private Frequency _frequencyMissionControl;

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnStateComplete;

    [ServerRpc(RequireOwnership = false)]
    public void CheckFrequenciesServerRpc()
    {
        if (_frequencyLauncher.IsTargetFrequency.Value && _frequencyMissionControl.IsTargetFrequency.Value)
        {
            OnStateComplete?.Invoke();
            GameState.Instance.ChangeState(GameState.GAMESTATES.SIMONSAYS);
        }
    }

    public void StartState()
    {
        OnStateStart?.Invoke();
    }
}
