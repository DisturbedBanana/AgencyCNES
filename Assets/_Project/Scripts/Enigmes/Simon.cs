using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class Simon : NetworkBehaviour
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

    [SerializeField] private List<SimonSpotLight> _colorsSpotLights = new List<SimonSpotLight>(4);
    [SerializeField] private List<Light> _spotLights = new List<Light>();

    private int _currentLevel = 0;
    private List<SimonColor> _colorsStackEnteredByPlayer = new List<SimonColor>();

    private Coroutine _colorRoutine = null;
    [SerializeField, Range(0, 5)] private float _holdColorTime = 2f;
    [SerializeField, Range(0, 10)] private float _pauseAfterColors = 3f;

    private NetworkVariable<bool> _canChooseColor = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool CanChooseColor { get => _canChooseColor.Value; set => _canChooseColor.Value = value; }


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
            EndSimonClientRpc();
            
        }
        else if (!isOrderCorrect)
        {
            ChangeLevelClientRpc(0);
            StartSimonClientRpc();
        }

        //_canChooseColor = currentLevel < (_levelList.Count - 1);
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
        StartSimonClientRpc();
    }

    [ClientRpc]
    public void StartSimonClientRpc()
    {
        StopSimonRoutine();
        if(_colorRoutine == null)
            _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel));
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
        if (NetworkManager.Singleton.IsHost)
        {
            _canChooseColor.Value = false;
            GameState.instance.ChangeState(GameState.GAMESTATES.FUSES);
        }
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
        foreach (var item in _spotLights)
        {
            item.color = Color.white;
        }
    }

    private Color ChooseColor(SimonColor simon) => _colorsSpotLights.First(spotLight => spotLight.SimonColor.Equals(simon)).Color;

    private void ChangeSpotLightsColor(SimonColor simonColor)
    {
        for (int i = 0; i < _spotLights.Count; i++)
        {
            _spotLights[i].color = ChooseColor(simonColor);
        }
    }
}
