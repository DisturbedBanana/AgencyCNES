using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseSocket : MonoBehaviour
{
    [SerializeField] int _ID;

    public void AttachFuse()
    {
        FuseManager.Instance.ActivateFuseLightRpc(_ID);
    }

    public void DetachFuse()
    {
        FuseManager.Instance.DeactivateFuseLightRpc(_ID);
    }
}
