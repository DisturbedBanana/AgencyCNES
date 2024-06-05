using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseSocket : MonoBehaviour
{
    [SerializeField] int _ID;

    public void AttachedFuse()
    {
        Debug.LogError("Attached Fuse" + _ID);
        //FuseManager.Instance.ActivateFuseLightRpc(_ID);
    }
}
