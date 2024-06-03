using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;

public class Separation : NetworkBehaviour
{
    [Header("Levers")]
    [SerializeField] private XRLever _leverFusee;
    private NetworkVariable<bool> _leverFuseeIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private XRLever _leverMissionControl;
    private NetworkVariable<bool> _leverMissionControlIsActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    [Header("Separation")]
    public UnityEvent OnSeparation;

    public void LeverActivated(int playerNumber)
    {
        NetworkVariable<bool> level = playerNumber == 0 ? _leverFuseeIsActivated : _leverMissionControlIsActivated;
        level.Value = true;

        if (GameState.instance.CurrentGameState == GameState.GAMESTATES.SEPARATION)
        {
            CheckSeparationLeverServerRpc(NetworkManager.Singleton.LocalClientId);
            Debug.LogError("Clicked button" + NetworkManager.Singleton.LocalClientId);
        }

    }

    public void LeverDeactivated(int playerNumber)
    {
        _leverFusee.value = false;

        NetworkVariable<bool> level = playerNumber == 0 ? _leverFuseeIsActivated : _leverMissionControlIsActivated;
        level.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckSeparationLeverServerRpc(ulong clientID)
    {
        if (!_leverFuseeIsActivated.Value || !_leverMissionControlIsActivated.Value)
            return;

        GameState.instance.ChangeState(GameState.GAMESTATES.WHACKAMOLE);

        // TODO: ouvrir la porte de l'ATV
        OnSeparation.Invoke();

    }

}
