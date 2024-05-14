using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class TestScript : NetworkBehaviour
{
    public NetworkManager networkManager;
    public GameObject prefabObject;

    public void CallDebug()
    {

    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Connected");
    }

    public void SpawnCube()
    {
        GameObject go = Instantiate(prefabObject, transform.position, Quaternion.identity);
        go.transform.localScale = Vector3.one * 30;
        go.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, false);
    }

    public void CallSelect(SelectEnterEventArgs args)
    {
        if(IsClient)
        {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            if(networkObject != null)
            {

                OwnerChangeServerRpc(networkObject, OwnerClientId);
            }
        }
            

    }

    [ServerRpc(RequireOwnership = false)]
    private void OwnerChangeServerRpc(NetworkObjectReference networkObjectRef, ulong newClientId)
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

    [ClientRpc]
    private void OwnerChangeClientRpc(NetworkObjectReference networkObjectRef, ulong clientID)
    {

        Debug.Log("non");
        
    }

    public void PushPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10f, ForceMode.Impulse);
    }
}
