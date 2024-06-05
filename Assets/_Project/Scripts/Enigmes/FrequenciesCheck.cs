using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FrequenciesCheck : NetworkBehaviour
{
    [SerializeField] private Frequency _frequencyLauncher;
    [SerializeField] private Frequency _frequencyMissionControl;

    [ServerRpc(RequireOwnership = false)]
    public void CheckFrequenciesServerRpc()
    {
        if (_frequencyLauncher.IsTargetFrequency.Value && _frequencyMissionControl.IsTargetFrequency.Value)
            GameState.instance.ChangeState(GameState.GAMESTATES.DODGE);
    }
}
