using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FuseManager : NetworkBehaviour
{
    public static FuseManager Instance;

    [SerializeField] List<FuseLight> _fuseLightList = new List<FuseLight>();
    private int _greenLightAmountRequired = 4;
    private int _currentGreenLightAmount;

    private void Awake()
    {
        if (Instance != null)
            Instance = this;
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].ActivateLight();
        _currentGreenLightAmount++;
    }

    [Rpc(SendTo.Everyone)]
    public void DeactivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].DeactivateLight();
        _currentGreenLightAmount--;
    }
}
