using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using NaughtyAttributes;

public class FuseManager : NetworkBehaviour
{
    public static FuseManager Instance;

    [Header("Color Materials")]
    [SerializeField] Material _greenMaterial;
    [SerializeField] Material _yellowMaterial;
    [SerializeField] Material _redMaterial;
    [Space()]
    [Header("Other Parameters")]
    [SerializeField] List<FuseLight> _fuseLightList = new List<FuseLight>();
    [SerializeField] List<GameObject> _lightsOnFuseBox = new List<GameObject>();
    private int _greenLightAmountRequired = 4;
    private int _currentGreenLightAmount;

    #region PROPERTIES
    public Material GreenMat
    {
        get { return _greenMaterial; }
    }

    public Material YellowMat
    {
        get { return _yellowMaterial; }
    }

    public Material RedMat
    {
        get { return _redMaterial; }
    }
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        foreach (GameObject item in _lightsOnFuseBox)
        {
            item.SetActive(false);
        }
    }

    //[Rpc(SendTo.Everyone)]
    public void ActivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].ActivateLight();
        _currentGreenLightAmount++;
        UpdateFuseBoxLights(true);
    }

    //[Rpc(SendTo.Everyone)]
    public void DeactivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].DeactivateLight();
        UpdateFuseBoxLights(false);
        _currentGreenLightAmount--;
    }

    public void UpdateFuseBoxLights(bool shouldAddLight)
    {
        if (shouldAddLight)
            _lightsOnFuseBox[_currentGreenLightAmount - 1].SetActive(true);
        else
            _lightsOnFuseBox[_currentGreenLightAmount - 1].SetActive(false);
    }
}
