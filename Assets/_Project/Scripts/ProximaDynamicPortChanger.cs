using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Proxima;

public class ProximaDynamicPortChanger : MonoBehaviour
{
    [SerializeField] private ProximaInspector _proximaInspector;
    
    private void Awake()
    {
        int generatedPort = Random.Range(7000, 8000);
        _proximaInspector.Port = generatedPort;
        GetComponent<TMP_Text>().text = "Port: " + generatedPort.ToString();
        Debug.Log("Proxima Port: " + generatedPort);
    }
}
