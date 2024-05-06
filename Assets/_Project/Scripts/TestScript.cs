using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class TestScript : NetworkBehaviour
{
    public NetworkManager networkManager;

    public void CallDebug()
    {

    }

    public void CallSelect(SelectEnterEventArgs args)
    {
        var networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

        //Debug.Log(networkManager.LocalClientId +"+"+ OwnerClientId);
        ////args.interactableObject.transform.GetComponent<NetworkObject>().ChangeOwnership(networkManager.LocalClientId);
        ////args.interactableObject.transform.GetComponent<NetworkObject>().ow
        //if (networkObject.OwnerClientId == OwnerClientId)
        //    return;

        //networkObject.ChangeOwnership(networkManager.LocalClientId);
        ServerOwnerChangeServerRpc(networkObject, default);

    }

    //[Rpc(SendTo.Server)]
    //public void ServerOwnerChangeRpc(ulong ownerID, NetworkObject networkObject)
    //{
    //    networkObject.ChangeOwnership(ownerID);
    //}

    [ServerRpc(RequireOwnership = false)]
    private void ServerOwnerChangeServerRpc(NetworkObjectReference networkObjectRef, ServerRpcParams rpcParams)
    {
        if (IsOwner)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if (networkObjectRef.TryGet(out NetworkObject networkObject2))
            {
                networkObject2.ChangeOwnership(clientId);
                Debug.Log("SERVER: " + clientId + " is driving the car.");
            }
            else
            {
                Debug.LogError("Didn't get car");
            }
        }
    }
}
