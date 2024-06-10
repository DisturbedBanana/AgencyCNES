using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ownership : NetworkBehaviour
{
    public void AskOwnership(NetworkObject networkObjectRef)
    {
        AskOwnershipServerRpc(networkObjectRef, NetworkManager.Singleton.LocalClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AskOwnershipServerRpc(NetworkObjectReference networkObjectRef, ulong newClientId)
    {

        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.ChangeOwnership(newClientId);
            Debug.Log("SERVER: " + networkObject2.OwnerClientId + " is driving the car.");
        }
        else
        {
            Debug.LogError("Didn't get car");
        }

    }
}
