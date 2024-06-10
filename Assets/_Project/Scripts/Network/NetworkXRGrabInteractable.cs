using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;
#if BURST_PRESENT
using Unity.Burst;
#endif

public class NetworkXRGrabInteractable : XRGrabInteractable
{
    //private readonly NetworkVariable<Vector3> _netPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    //private readonly NetworkVariable<Quaternion> _netRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    private ObjectSync objectSync;
    private void Start()
    {
        objectSync = GetComponent<ObjectSync>();
    }

    protected override void Grab()
    {
        base.Grab();
        objectSync.ObjectGrabbed();
    }
    protected override void Drop()
    {
        base.Drop();
        objectSync.ObjectDropped();
    }

    //private void Update()
    //{
    //    if (IsOwner)
    //    {
    //        _netPos.Value = transform.position;
    //        _netRot.Value = transform.rotation;
    //    }
    //    else
    //    {
    //        transform.position = _netPos.Value;
    //        transform.rotation = _netRot.Value;
    //    }
    //}
}
