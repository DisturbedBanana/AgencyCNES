using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Renderer[] meshesToDisable;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            foreach (Renderer item in meshesToDisable)
            {
                item.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position;
            head.rotation = VRRigReferences.Singleton.head.rotation;

            leftHand.position = leftHand.gameObject.activeSelf ? VRRigReferences.Singleton.leftHand.position : VRRigReferences.Singleton.leftController.position;
            leftHand.rotation = leftHand.gameObject.activeSelf ? VRRigReferences.Singleton.leftHand.rotation : VRRigReferences.Singleton.leftController.rotation;

            rightHand.position = rightHand.gameObject.activeSelf ? VRRigReferences.Singleton.rightHand.position : VRRigReferences.Singleton.rightController.position;
            rightHand.rotation = rightHand.gameObject.activeSelf ? VRRigReferences.Singleton.rightHand.rotation : VRRigReferences.Singleton.rightController.rotation;

        }
    }

}
