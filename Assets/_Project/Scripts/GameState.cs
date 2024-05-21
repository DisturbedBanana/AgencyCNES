using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class GameState : NetworkBehaviour
{
    public static GameState instance;

    int _stateIndex = 0;

    public int StateIndex
    {
        get { return _stateIndex; }
        set { _stateIndex = value; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextState()
    {
        if (IsOwner)
        {
            AskForGameStateUpdateClientRpc(this.GetComponent<NetworkObject>());
        }
        else
        {
            AskForGameStateUpdateServerRpc(this.GetComponent<NetworkObject>());
        }
        
        SwitchState();
    }

    public void SwitchState()
    {
        _stateIndex++;


        switch (_stateIndex)
        {
            case 1:
                //Password found
                //Must calibrate tool
                break;
            case 2:
                //Tool calibrated
                //Must attach harness
                break;
            case 3:
                //Harness attached
                //Must launch rocket
                break;
            case 4:
                //Launched rocket
                break;
            case 5:
                
                break;
            default:
                break;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AskForGameStateUpdateServerRpc(NetworkObjectReference networkObjectRef)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().SwitchState();
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = true)]
    private void AskForGameStateUpdateClientRpc(NetworkObjectReference networkObjectRef)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().SwitchState();
        }
    }
}
