using System;
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

    [Header("Events")]
    public UnityEvent OnComplete;
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

        GameState.Instance.ChangeState(GameState.GAMESTATES.WHACKAMOLE);

        OnComplete?.Invoke();// TODO: ouvrir la porte de l'ATV

    }

    private bool AreBothLeverActivated() => _leverFuseeIsActivated.Value && _leverMissionControlIsActivated.Value;
    private NetworkVariable<bool> GetPlayerLever(int playerNumber) => playerNumber == 0 ? _leverFuseeIsActivated : _leverMissionControlIsActivated;



}
