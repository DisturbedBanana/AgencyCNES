using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkConnect : MonoBehaviour
{
    public TextMeshProUGUI m_TextMeshProUGUI;
    public void Create()
    {
        NetworkManager.Singleton.StartHost();
        m_TextMeshProUGUI.text += "StartHost NetworkConnect\n";
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
        m_TextMeshProUGUI.text += "StartClient NetworkConnect\n";
    }
}
