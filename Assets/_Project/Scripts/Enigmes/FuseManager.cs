using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

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
    private int _currentGreenFusesActivated;
    private int _currentConnectedFuses;
    private bool _areFusesSolved = false;

    [Header("Events")]
    public UnityEvent OnFuseConnected;
    public UnityEvent OnFuseDeconnected;
    public UnityEvent OnCorrectFuseConnected;
    public UnityEvent OnCorrectFuseDeconnected;
    public UnityEvent OnComplete;

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

    [Rpc(SendTo.Everyone)]
    public void ActivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].ActivateLight();
        Debug.LogError("Activated light number: " + ID);
        UpdateFuseBoxLights(true);
        _currentConnectedFuses++;
        OnFuseConnected?.Invoke();

        if (_fuseLightList[ID].FuseLightColor != FuseLight.AvailableColors.Green)
            return;

        _currentGreenFusesActivated++;
        OnCorrectFuseConnected?.Invoke();
    }

    [Rpc(SendTo.Everyone)]
    public void DeactivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].DeactivateLight();
        UpdateFuseBoxLights(false);
        _currentConnectedFuses--;
        OnFuseDeconnected?.Invoke();

        if (_fuseLightList[ID].FuseLightColor != FuseLight.AvailableColors.Green)
            return;

        if(_currentGreenFusesActivated > 0)
        {
            _currentGreenFusesActivated--;
            OnCorrectFuseDeconnected?.Invoke();
        }
    }

    private void UpdateFuseBoxLights(bool shouldAddLight)
    {
        if (shouldAddLight)
            _lightsOnFuseBox[_currentConnectedFuses - 1].SetActive(true);
        else
            _lightsOnFuseBox[_currentConnectedFuses - 1].SetActive(false);
    }

    public void ValidatePuzzle()
    {
        if (!AreFusesSolved())
            return;

        OnComplete?.Invoke();
        GameState.Instance.ChangeState(GameState.GAMESTATES.SEPARATION);
    }

    private bool AreFusesSolved()
    {
        return _currentGreenFusesActivated >= 4;
    }
}
