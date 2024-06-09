using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public enum EnigmaEvents
{
    ONSUCCEED = 0,
    ONFAILED = 1,
    OnStateComplete = 2
}
public class Simon : NetworkBehaviour, IGameState
{
    #region Structs

    [Serializable]
    struct SimonLevel
    {
        public List<SimonColor> ColorOrder;
    }

    [Serializable]
    struct SimonSpotLight
    {
        public SimonColor SimonColor;
        public Color Color;
    }
    #endregion

    public enum SimonColor
    {
        GREEN = 0,
        RED,
        BLUE,
        YELLOW
    }

    [SerializeField] private List<SimonLevel> _levelList = new List<SimonLevel>();

    [SerializeField] private List<SimonSpotLight> _colors = new List<SimonSpotLight>(4);
    [SerializeField] private List<Light> _colorSpotLights = new List<Light>();

    [Header("Ambiant light")]
    [SerializeField] private List<Light> _ambiantSpotLights = new List<Light>();
    private float _ambiantSpotLightIntensity;
    [SerializeField] private float _dimAmbiantIntensity;

    private int _currentLevel = 0;

    private List<SimonColor> _colorsStackEnteredByPlayer = new List<SimonColor>();

    private Coroutine _colorRoutine = null;
    [SerializeField, Range(0, 5)] private float _holdColorTime = 2f;
    [SerializeField, Range(0, 10)] private float _pauseAfterColors = 3f;

    private NetworkVariable<bool> _canChooseColor = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool CanChooseColor { get => _canChooseColor.Value; set => _canChooseColor.Value = value; }

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnLevelSucceed;
    public UnityEvent OnLevelFailed;
    public UnityEvent OnStateComplete;

    private void Reset()
    {
        _dimAmbiantIntensity = 0.5f;
    }

    private void Start()
    {
        _ambiantSpotLightIntensity = _ambiantSpotLights.Count == 0 ? 1f : _ambiantSpotLights[0].intensity;
    }

    public void PushButton(string color)
    {
        if (!_canChooseColor.Value)
            return;

        foreach (var enumValue in Enum.GetValues(typeof(SimonColor)))
        {
            if (enumValue.ToString() == color.ToUpper())
            {
                _colorsStackEnteredByPlayer.Add((SimonColor)enumValue);
            }
        }

        List<int> colorsValue = new List<int>();
        foreach (var item in _colorsStackEnteredByPlayer)
        {
            colorsValue.Add((int)item);
        }

        if (_isColorOrderFinished)
            ColorsOrderVerificationServerRPC(colorsValue.ToArray());

    }

    private bool _isColorOrderFinished => _colorsStackEnteredByPlayer.Count == _levelList[_currentLevel].ColorOrder.Count;

    [ServerRpc(RequireOwnership = false)]
    private void ColorsOrderVerificationServerRPC(int[] colorsByPlayer)
    {
        bool isOrderCorrect = true;
        for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
        {
            if ((SimonColor)colorsByPlayer[i] != _levelList[_currentLevel].ColorOrder[i])
            {
                isOrderCorrect = false;
                break;
            }
        }
        Debug.Log(isOrderCorrect ? "Order correct!" : "Order is not good!");

        PlayEventClientRpc(isOrderCorrect ? EnigmaEvents.ONSUCCEED : EnigmaEvents.ONFAILED);

        ClearPlayerColorsClientRpc();
        DisableAllLightsClientRpc();


        if (isOrderCorrect && _currentLevel < (_levelList.Count - 1))
        {
            ChangeLevelClientRpc(_currentLevel + 1);
            StartSimonClientRpc();
            Debug.Log("Next level is " + _currentLevel);
        } 
        else if (isOrderCorrect && _currentLevel >= (_levelList.Count - 1))
        {
            _canChooseColor.Value = false;
            GameState.Instance.ChangeState(GameState.GAMESTATES.FUSES);
            EndSimonClientRpc();
            PlayEventClientRpc(EnigmaEvents.ONFAILED);

        }
        else if (!isOrderCorrect)
        {
            StartSimonClientRpc();
        }

    }

    [ClientRpc]
    private void PlayEventClientRpc(EnigmaEvents eventNumber)
    {
        Debug.Log("Event: " + eventNumber);

        switch (eventNumber) {
            
            case EnigmaEvents.ONSUCCEED:
                OnLevelSucceed?.Invoke();
                break;
            case EnigmaEvents.ONFAILED:
                OnLevelFailed?.Invoke();
                break;
            case EnigmaEvents.OnStateComplete:
                OnStateComplete?.Invoke();
                break;

        }
    }

    [ClientRpc]
    private void ChangeLevelClientRpc(int level)
    {
        _currentLevel = level;
    }

    [ClientRpc]
    private void ClearPlayerColorsClientRpc()
    {
        _colorsStackEnteredByPlayer.Clear();
    }

    public void StartSimon()
    {
        _canChooseColor.Value = true;
        ChangeAmbiantLights(true);
        StartSimonClientRpc();
    }

    [ClientRpc]
    public void StartSimonClientRpc()
    {
        StopSimonRoutine();
        if(_colorRoutine == null)
            _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel));
    }

    public void StartState()
    {
        OnStateStart?.Invoke();
        CanChooseColor = true;
        ChangeAmbiantLights(true);
        StartSimonClientRpc();
    }

    private void ChangeAmbiantLights(bool changeToDimLights)
    {
        foreach (var spotLight in _ambiantSpotLights)
        {
            spotLight.intensity = changeToDimLights ? _dimAmbiantIntensity : _ambiantSpotLightIntensity;
        }
    }

    private void StopSimonRoutine()
    {
        if (_colorRoutine != null)
        {
            StopCoroutine(_colorRoutine);
            _colorRoutine = null;
        }
    }

    [ClientRpc]
    private void EndSimonClientRpc()
    {
        StopSimonRoutine();
        Debug.Log("It was the last level");
        ChangeAmbiantLights(changeToDimLights: false);
    }

    private IEnumerator DisplayColorRoutine(int level)
    {
        var wait1 = new WaitForSeconds(_holdColorTime);
        var wait2 = new WaitForSeconds(_pauseAfterColors);
        while (_canChooseColor.Value)
        {
            for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
            {
                ChangeSpotLightsColor(_levelList[_currentLevel].ColorOrder[i]);
                yield return wait1;
                DisableAllLightsClientRpc();
            }
            yield return wait2;
        }
        
    }

    [ClientRpc]
    private void DisableAllLightsClientRpc()
    {
        foreach (var item in _colorSpotLights)
        {
            item.color = new Color(0,0,0,0);
        }
    }

    private Color ChooseColor(SimonColor simon) => _colors.First(spotLight => spotLight.SimonColor.Equals(simon)).Color;

    private void ChangeSpotLightsColor(SimonColor simonColor)
    {
        for (int i = 0; i < _colorSpotLights.Count; i++)
        {
            _colorSpotLights[i].color = ChooseColor(simonColor);
        }
    }
}
