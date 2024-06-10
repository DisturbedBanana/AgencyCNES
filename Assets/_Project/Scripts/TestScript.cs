using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[Serializable]
public struct ObjectMultiSync : INetworkSerializable
{
    public Vector3 Position; 
    public Quaternion Rotation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Array
        if (serializer.IsReader)
        {
            Position = new Vector3();
        }

        serializer.SerializeValue(ref Position);


        // Array
        if (serializer.IsReader)
        {
            Rotation = new Quaternion();
        }

        serializer.SerializeValue(ref Rotation);
        
    }
}

public class TestScript : NetworkBehaviour
{
    public GameObject prefabObject;
    private GameObject _currentSelectedObject = null;
    public bool SpawnCubeAtStart = false;
    
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
        //NetworkXRGrabInteractable.targetMove += ClientObjectMove;
    }

    private void OnDisable()
    {
        //NetworkXRGrabInteractable.targetMove -= ClientObjectMove;

    }

    private void ClientObjectMove(ObjectMultiSync objectToSend)
    {
        AskServerForObjectMovementServerRpc(_currentSelectedObject.transform.GetComponent<NetworkObject>(), NetworkManager.Singleton.LocalClientId, objectToSend);
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost && SpawnCubeAtStart)
            SpawnCube();
    }

    public void SpawnCube()
    {
        GameObject go = Instantiate(prefabObject, transform.position, Quaternion.identity);
        go.transform.localScale = Vector3.one * 30;
        go.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, false);
    }

    public void CallSelect(SelectEnterEventArgs args)
    {
        NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();
        _currentSelectedObject = networkObject.gameObject;
        if (networkObject != null)
        {
            if(networkObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
                OwnerChangeServerRpc(networkObject, NetworkManager.Singleton.LocalClientId);
            if(networkObject.CompareTag("GravityObject"))
                ObjectGravityEnabledClientRpc(false, networkObject);

        }


    }

    public void RemoveSelect(SelectExitEventArgs args)
    {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                RemoveObjectMovementServerRpc(networkObject, NetworkManager.Singleton.LocalClientId, _currentSelectedObject.transform.position);
            }
        
        _currentSelectedObject = null;

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

    [ServerRpc(RequireOwnership = false)]
    private void AskServerForObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID, ObjectMultiSync objectToSend)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject))
        {
            //networkObject.transform.position = pos; 
            GetRigidbody(networkObjectRef).MovePosition(objectToSend.Position);
            GetRigidbody(networkObjectRef).MoveRotation(objectToSend.Rotation);
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
        Debug.Log("RemoveServerForObjectMovementServerRpc");
        networkObjectRef.TryGet(out NetworkObject networkObject);
        if (networkObject.CompareTag("GravityObject"))
            ObjectGravityEnabledClientRpc(true, networkObjectRef);
    }

    [ClientRpc]
    private void ObjectGravityEnabledClientRpc(bool isEnabled, NetworkObjectReference networkObjectRef)
    {

        networkObjectRef.TryGet(out NetworkObject networkObject);
        GetRigidbody(networkObjectRef).useGravity = isEnabled;
    }

    private Rigidbody GetRigidbody(NetworkObjectReference networkObjectRef)
    {
        if (!networkObjectRef.TryGet(out NetworkObject networkObject))
            return null;
        
        
        if (networkObject.TryGetComponent(out Rigidbody rb))
        {
            return rb;
        }
        else if (networkObject.GetComponentInChildren<Rigidbody>() != null)
        {
            return networkObject.GetComponentInChildren<Rigidbody>();
        }

        return null;
    }


    public void PushPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10f, ForceMode.Impulse);
    }
}


