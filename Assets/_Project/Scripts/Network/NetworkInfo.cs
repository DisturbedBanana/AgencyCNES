using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkInfo : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _pingText;
    private ulong _ping;

    void Update()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
            return;

        _ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
        _pingText.text = _ping.ToString();
    }
}
