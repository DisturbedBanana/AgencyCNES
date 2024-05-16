using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class TestScript : NetworkBehaviour
{
    public NetworkManager networkManager;
    public GameObject prefabObject;
    public Transform RightController;
    private GameObject _currentSelectedObject = null;

    private void Update()
    {
        //if(time >= timeBetweenTicks)
        //{
        //    _shouldUpdateObject = true;
        //    time = 0;
        //}
        //time += Time.deltaTime;
        
    }

    private void OnEnable()
    {
        NetworkXRGrabInteractable.tagetMove += ClientObjectMove;
    }

    private void OnDisable()
    {
        NetworkXRGrabInteractable.tagetMove -= ClientObjectMove;

    }

    private void ClientObjectMove(Vector3 pos)
    {
        AskServerForObjectMovementServerRpc(_currentSelectedObject.transform.GetComponent<NetworkObject>(), OwnerClientId, pos);
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
            SpawnCube();
    }

    public void SpawnCube()
    {
        GameObject go = Instantiate(prefabObject, transform.position, Quaternion.identity);
        go.transform.localScale = Vector3.one * 30;
        go.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, false);
    }

    public void CallSelect(SelectEnterEventArgs args)
    {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            _currentSelectedObject = networkObject.gameObject;
            if (networkObject != null)
            {
                Debug.Log(OwnerClientId + " owner id");
                OwnerChangeServerRpc(networkObject, OwnerClientId);

            }
        

    }

    public void RemoveSelect(SelectExitEventArgs args)
    {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                RemoveObjectMovementServerRpc(networkObject, OwnerClientId, _currentSelectedObject.transform.position);
            }
        
        _currentSelectedObject = null;

    }

    [ServerRpc(RequireOwnership = false)]
    private void OwnerChangeServerRpc(NetworkObjectReference networkObjectRef, ulong newClientId)
    {

        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.ChangeOwnership(newClientId);
            ObjectGravityEnabledClientRpc(false, networkObjectRef);
            Debug.Log("SERVER: " + networkObject2.OwnerClientId + " is driving the car.");
        }
        else
        {
            Debug.LogError("Didn't get car");
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void AskServerForObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID, Vector3 pos)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject))
        {
            //_currentObject = GameObject.Find(networkObject.gameObject.name);
            networkObject.transform.position = pos;
            if(IsOwner)
                AskServerForObjectMovementClientRpc(networkObjectRef, clientID, pos);
            //moveRoutine = StartCoroutine(SendToClientMovement(networkObject));
        }
    }
    [ClientRpc]
    private void AskServerForObjectMovementClientRpc(NetworkObjectReference networkObjectRef, ulong clientID, Vector3 pos)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject))
        {
            networkObject.transform.position = pos;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID, Vector3 pos)
    {
        networkObjectRef.TryGet(out NetworkObject networkObject);
        Debug.Log("RemoveServerForObjectMovementServerRpc");
        ObjectGravityEnabledClientRpc(true, networkObjectRef);
        //networkObject.transform.position = pos;
    }

    [ClientRpc]
    private void ObjectGravityEnabledClientRpc(bool isEnabled, NetworkObjectReference networkObjectRef)
    {

        networkObjectRef.TryGet(out NetworkObject networkObject);
        if (networkObject.GetComponent<Rigidbody>().useGravity == isEnabled)
            return;

        networkObject.GetComponent<Rigidbody>().useGravity = isEnabled;
    }


    public void PushPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10f, ForceMode.Impulse);
    }
}


