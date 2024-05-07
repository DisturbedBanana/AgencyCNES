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

    public override void OnNetworkSpawn()
    {
        Debug.Log("Connected");
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
        if (networkObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            OwnerChangeServerRpc(networkObject, default);

    }

    //[Rpc(SendTo.Server)]
    //public void ServerOwnerChangeRpc(ulong ownerID, NetworkObject networkObject)
    //{
    //    networkObject.ChangeOwnership(ownerID);
    //}

    [ServerRpc(RequireOwnership = false)]
    private void OwnerChangeServerRpc(NetworkObjectReference networkObjectRef, ServerRpcParams rpcParams)
    {
        
        Debug.Log("oui");
            ulong clientId = rpcParams.Receive.SenderClientId;

            if (networkObjectRef.TryGet(out NetworkObject networkObject2))
            {
                networkObject2.RemoveOwnership();
                networkObject2.ChangeOwnership(clientId);
                Debug.Log("SERVER: " + networkObject2.OwnerClientId + " is driving the car.");
            }
            else
            {
                Debug.LogError("Didn't get car");
            }
        
    }

    public void PushPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10f, ForceMode.Impulse);
    }
}
