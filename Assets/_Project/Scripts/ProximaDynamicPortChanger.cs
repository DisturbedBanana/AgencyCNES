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
        Debug.Log(_proximaInspector.Port);
        GetComponent<TMP_Text>().text = "Port: " + _proximaInspector.Port;
        //_proximaInspector.Port = generatedPort;
        //Debug.Log("Proxima Port: " + generatedPort);
    }
}
