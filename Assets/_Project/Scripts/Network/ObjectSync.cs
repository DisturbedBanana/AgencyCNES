using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectSync : NetworkBehaviour
{
    private NetworkVariable<RigidObjectSyncPos> _netObject = new NetworkVariable<RigidObjectSyncPos>(writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _cheapInterpolationTime = 0.1f;

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

    private void Awake()
    {

    }

    public void ObjectGrabbed()
    {

        //AskOwnership(GetComponent<NetworkObject>());
        isGrabbed = true;
        Debug.Log("Object grabbed");
    }
    public void ObjectDropped()
    {
        isGrabbed = false;
        Debug.Log("Object dropped");
    }

    bool isGrabbed = false;

    private void Update()
    {
        //if (!isGrabbed)
        //    return;

        if (IsOwner) TransmitState();
        else ConsumeState();
        
    }

    private Vector3 _posVel;
    private float _rotVelY;
    private void ConsumeState()
    {
        _rb.MovePosition(Vector3.SmoothDamp(_rb.position, _netObject.Value.Position, ref _posVel, _cheapInterpolationTime));

        transform.rotation = Quaternion.Euler(
            0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _netObject.Value.Rotation.y, ref _rotVelY, _cheapInterpolationTime), 0);

        _rb.useGravity = _netObject.Value.UseGravity;
    }

    private void TransmitState()
    {
        var state = new RigidObjectSyncPos
        {
            Position = _rb.position,
            Rotation = _rb.rotation,
            UseGravity = !isGrabbed
        };
        if(NetworkManager.Singleton.IsServer)
            _netObject.Value = state;
         else
            TransmitStateServerRpc(state);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TransmitStateServerRpc(RigidObjectSyncPos rigidObjectSync)
    {
        _netObject.Value = rigidObjectSync;
    }

    [Serializable]
    public struct RigidObjectSyncPos: INetworkSerializable
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private bool _useGravity;

        internal Vector3 Position
        {
            get => _position;
            set => _position = value;
        }

        internal Quaternion Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        internal bool UseGravity
        {
            get => _useGravity;
            set => _useGravity = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _position );
            serializer.SerializeValue(ref _rotation);
            serializer.SerializeValue(ref _useGravity);
        }
    }

}
