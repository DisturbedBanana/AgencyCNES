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
using UnityEngine.UI;

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
        YELLOW,
        BLACK
    }

    [SerializeField] private List<SimonLevel> _levelList = new List<SimonLevel>();

    [SerializeField] private List<SimonSpotLight> _colors = new List<SimonSpotLight>(4);
    [SerializeField] private List<Light> _colorSpotLights = new List<Light>();
    [SerializeField] private List<Light> _validationLightLevel = new List<Light>();

    [Header("Screen Images")]
    [SerializeField] private GameObject _layoutImageScreen;
    [SerializeField] private GameObject _imageScreenPrefab;

    [Header("Ambiant light")]
    [SerializeField] private List<Light> _ambiantSpotLights = new List<Light>();
    private float _previousAmbiantSpotLightIntensity;
    [SerializeField] private float _dimAmbiantIntensity;

    private int _currentLevel = 0;

    private List<SimonColor> _colorsStackEnteredByPlayer = new List<SimonColor>();

    private Coroutine _colorRoutine = null;
    [SerializeField, Range(0, 5)] private float _holdColorTime = 2f;
    [SerializeField, Range(0, 10)] private float _pauseAfterEachColor = 0.25f;
    [SerializeField, Range(0, 10)] private float _pauseAfterColorSequence = 3f;
    [SerializeField, Range(0, 10)] private float _waitBeforeClearButtons = 2f;
    [SerializeField, Range(0, 10)] private float _replaySimonSpeed = 0.2f;

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
        _previousAmbiantSpotLightIntensity = _ambiantSpotLights.Count == 0 ? 1f : _ambiantSpotLights[0].intensity;
    }

    public void PushButton(string color)
    {
        
        if (!_canChooseColor.Value || GameState.Instance.CurrentGameState != GameState.GAMESTATES.SIMONSAYS)
            return;

        foreach (var enumValue in Enum.GetValues(typeof(SimonColor)))
        {
            if (enumValue.ToString() == color.ToUpper())
            {
                _colorsStackEnteredByPlayer.Add((SimonColor)enumValue);
                var imageGO = Instantiate(_imageScreenPrefab, _layoutImageScreen.transform);
                imageGO.GetComponent<Image>().color = ChooseColor((SimonColor)enumValue);
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
        GameState.Instance.FlashValidationLightRpc(isOrderCorrect);

        ClearPlayerColorsClientRpc();
        DisableAllLightsClientRpc();

        if(isOrderCorrect)
            ValidateSimonBeforeChangingLevelRpc();
        else
            StartSimonClientRpc();
        

    }

    [ServerRpc(RequireOwnership = false)]
    private void VerifyNextLevelServerRPC()
    {
        if (!AreAllLevelsCompleted())
        {
            ChangeLevelClientRpc(_currentLevel + 1);
            StartSimonClientRpc();
            Debug.Log("Next level is " + _currentLevel);
        }
        else if (AreAllLevelsCompleted())
        {
            _canChooseColor.Value = false;
            EndSimonClientRpc();
            PlayEventClientRpc(EnigmaEvents.OnStateComplete);
            GameState.Instance.ChangeState(GameState.GAMESTATES.FUSES);

        }
    }

        private bool AreAllLevelsCompleted()
    {
        return _currentLevel >= (_levelList.Count - 1);
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
        if (_currentLevel < _validationLightLevel.Count)
            ChangeValidationLightLevel(_currentLevel, true);
        _currentLevel = level;
    }

    [Rpc(SendTo.Everyone)]
    private void ValidateSimonBeforeChangingLevelRpc()
    {
        if(_validateSimonBeforeChangingLevelCoroutine == null)
            _validateSimonBeforeChangingLevelCoroutine = StartCoroutine(ValidateSimonBeforeChangingLevel());
    }

    private Coroutine _validateSimonBeforeChangingLevelCoroutine = null;

    private IEnumerator ValidateSimonBeforeChangingLevel()
    {
        if(IsOwner)
            _canChooseColor.Value = false;

        var wait1 = new WaitForSeconds(_replaySimonSpeed);
        ChangeSpotLightsColor(SimonColor.BLACK);
        yield return wait1;
        for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
        {
            ChangeSpotLightsColor(_levelList[_currentLevel].ColorOrder[i]);
            yield return wait1;
        }

        ChangeSpotLightsColor(SimonColor.BLACK);
        yield return new WaitForSeconds(_waitBeforeClearButtons);
        if (IsOwner)
            _canChooseColor.Value = true;

        if (IsOwner)
            VerifyNextLevelServerRPC();

        _validateSimonBeforeChangingLevelCoroutine = null;
    }

    private void ChangeValidationLightLevel(int level, bool isValid)
    {
        _validationLightLevel[level].color = isValid ? Color.green : Color.red;
    }

    [ClientRpc]
    private void ClearPlayerColorsClientRpc()
    {
        StartCoroutine(WaitBeforeClearScreenButtonsColors());
    }

    private IEnumerator WaitBeforeClearScreenButtonsColors()
    {
        if(IsOwner)
            _canChooseColor.Value = false;

        yield return new WaitForSeconds(_waitBeforeClearButtons);

        _colorsStackEnteredByPlayer.Clear();
        foreach (Transform child in _layoutImageScreen.transform)
        {
            Destroy(child.gameObject);
        }

        if (IsOwner)
            _canChooseColor.Value = true;
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
        Debug.Log("StartSimonClientRpc");
        StopSimonRoutine();
        _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel));
    }

    public void StartState()
    {
        OnStateStart?.Invoke();
        if(IsOwner)
            _canChooseColor.Value = true;
        EnableColorSpotLights(true);
        ChangeAmbiantLights(true);
        StartSimonClientRpc();
    }

    private void EnableColorSpotLights(bool enable)
    {
        for (int i = 0; i < _colorSpotLights.Count; i++)
        {
            _colorSpotLights[i].enabled = enable;
        }
    }

    private void ChangeAmbiantLights(bool changeToDimLights)
    {
        foreach (var spotLight in _ambiantSpotLights)
        {
            spotLight.intensity = changeToDimLights ? _dimAmbiantIntensity : _previousAmbiantSpotLightIntensity;
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
        ChangeAmbiantLights(changeToDimLights: false);
        EnableColorSpotLights(false);
        ChangeValidationLightLevel(_currentLevel, true);
        Debug.Log("It was the last level");
    }

    private IEnumerator DisplayColorRoutine(int level)
    {
        Debug.Log("DisplayColorRoutine");
        var wait1 = new WaitForSeconds(_holdColorTime);
        var wait2 = new WaitForSeconds(_pauseAfterColorSequence);
        var waitAfterEachColor = new WaitForSeconds(_pauseAfterEachColor);
        while (true)
        {
            for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
            {
                ChangeSpotLightsColor(_levelList[_currentLevel].ColorOrder[i]);
                yield return wait1;

                ChangeSpotLightsColor(SimonColor.BLACK);
                yield return waitAfterEachColor;

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
