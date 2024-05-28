using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Simon : NetworkBehaviour
{

    #region Structs

    [Serializable]
    struct SimonLevel
    {
        public List<SimonColor> ColorOrder;
    }

    [Serializable]
    struct SimonLight
    {
        public SimonColor Color;
        public Light Light;
    }
    #endregion

    public enum SimonColor
    {
        GREEN = 0,
        RED,
        BLUE,
        YELLOW
    }

    [SerializeField] private List<SimonLight> _colorsLights = new List<SimonLight>(4);
    [SerializeField] private List<SimonLevel> _levelList = new List<SimonLevel>();
    private int _currentLevel = 0;
    private List<SimonColor> _colorsStackEnteredByPlayer = new List<SimonColor>();
    //private NetworkVariable<List<SimonColor>> _colorsStackEnteredByPlayer = new NetworkVariable<List<SimonColor>>();

    private Coroutine _colorRoutine = null;
    [SerializeField, Range(0, 5)] private float _holdColorTime = 2f;
    [SerializeField, Range(0, 10)] private float _pauseAfterColors = 3f;

    //private bool _canChooseColor = true;
    private NetworkVariable<bool> _canChooseColor = new NetworkVariable<bool>();
    public bool CanChooseColor { get => _canChooseColor.Value; set => _canChooseColor.Value = value; }


    public override void OnNetworkSpawn()
    {
        _canChooseColor.Value = false;
    }


    public void ButtonStartSimon()
    {
        if (NetworkManager.Singleton.ServerIsHost)
        {
            StartSimonClientRpc();
        }
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

        if (isColorOrderFinished)
            ColorsOrderVerificationServerRPC(colorsValue.ToArray());

    }

    private bool isColorOrderFinished => _colorsStackEnteredByPlayer.Count == _levelList[_currentLevel].ColorOrder.Count;

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

    [ClientRpc]
    public void StartSimonClientRpc()
    {
        StopSimonRoutine();
        _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel));
    }

    private void StopSimonRoutine()
    {
        if (_colorRoutine != null)
            StopCoroutine(_colorRoutine);
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
        while (_canChooseColor.Value)
        {
            for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
            {
                Light lightToEnable = ChooseLight(_levelList[_currentLevel].ColorOrder[i]);
                lightToEnable.enabled = true;
                yield return new WaitForSeconds(_holdColorTime);
                lightToEnable.enabled = false;
            }
            yield return new WaitForSeconds(_pauseAfterColors);
            yield return null;
        }
        
    }

    [ClientRpc]
    private void DisableAllLightsClientRpc()
    {
        foreach (var item in _colorsLights)
        {
            item.Light.enabled = false;
        }
    }

    private Light ChooseLight(SimonColor simon) => _colorsLights.First(color => color.Color.Equals(simon)).Light;
}
