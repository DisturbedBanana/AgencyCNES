using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class TestScript : NetworkBehaviour
    {
        public NetworkManager networkManager;
        public GameObject prefabObject;
        private bool _shouldUpdateObject = false;
        private SelectEnterEventArgs _currentSelectedObject = null;

        private void Update()
        {
            if (_shouldUpdateObject)
            {
                AskServerForObjectMovementServerRpc(_currentSelectedObject.interactableObject.transform.GetComponent<NetworkObject>(), OwnerClientId);
            }
        }

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
            _currentSelectedObject = args;
            if (IsClient)
            {
                NetworkObject networkObject = args.interactableObject.transform.GetComponent<NetworkObject>();

                if (networkObject != null)
                {

                    OwnerChangeServerRpc(networkObject, OwnerClientId);
                }
            }
            _shouldUpdateObject = true;

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
        private void AskServerForObjectMovementServerRpc(NetworkObjectReference networkObjectRef, ulong clientID)
        {
            if (networkObjectRef.TryGet(out NetworkObject networkObject))
            {
                networkObject.GetComponent<NetworkRigidbody>().transform.position = _currentSelectedObject.interactableObject.transform.position;
                SendObjectLocationToClientRpc(_currentSelectedObject.interactableObject.transform.position, networkObject);
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
}


