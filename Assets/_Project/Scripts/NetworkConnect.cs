using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkConnect : MonoBehaviour
{
    public void Create()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
