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
    private bool _shouldUpdateObject = false;
    private SelectEnterEventArgs _currentSelectedObject = null;
    private GameObject _currentObject = null;
    private Coroutine moveRoutine = null;
    private float timeBetweenTicks = 0.01f;
    private float time;

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
        NetworkXRGrabInteractable.tagetMove += ObjectMove;
    }

    private void OnDisable()
    {
        NetworkXRGrabInteractable.tagetMove -= ObjectMove;

    }

    private void ObjectMove(Vector3 pos)
    {
        //if (!_shouldUpdateObject)
        //    return;

        var obje = _currentSelectedObject.interactableObject;
        if(IsOwner)
            obje.transform.position = pos;
        AskServerForObjectMovementServerRpc(obje.transform.GetComponent<NetworkObject>(), OwnerClientId, pos);
        _shouldUpdateObject = false;
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
        _currentSelectedObject = args;
        if (IsClient)
        {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            _currentObject = networkObject.gameObject;
            if (networkObject != null)
            {

                OwnerChangeServerRpc(networkObject, OwnerClientId);

            }
        }
        _shouldUpdateObject = true;

    }

    public void RemoveSelect(SelectExitEventArgs args)
    {
        if (IsClient)
        {
            NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                RemoveServerForObjectMovementServerRpc(networkObject, OwnerClientId, _currentSelectedObject.interactableObject.transform.position);
            }
        }
        _currentObject = null;
        _shouldUpdateObject = false;

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
    private void AskServerForObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID, Vector3 pos)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject))
        {
            _currentObject = GameObject.Find(networkObject.gameObject.name);
            networkObject.transform.position = pos;
            //moveRoutine = StartCoroutine(SendToClientMovement(networkObject));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveServerForObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID, Vector3 pos)
    {
        networkObjectRef.TryGet(out NetworkObject networkObject);
        Debug.Log("RemoveServerForObjectMovementServerRpc");
        //networkObject.transform.position = pos;
        //StopCoroutine(moveRoutine);
        moveRoutine = null;
    }

    IEnumerator SendToClientMovement(NetworkObject networkObject)
    {
        _currentObject = GameObject.Find(networkObject.gameObject.name);
        while (true)
        {
            yield return new WaitForSeconds(0.02f);
            if (!_currentObject)
                break;
            networkObject.transform.position = _currentObject.transform.position;

        }
    }

    [ClientRpc]
    private void SendObjectLocationToClientRpc(Vector3 newPos, NetworkObjectReference networkObjectRef)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject))
        {
            networkObject.GetComponent<NetworkRigidbody>().transform.position = newPos;
        }
    }

    public void PushPlayer(GameObject player)
    {
        player.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10f, ForceMode.Impulse);
    }
}


