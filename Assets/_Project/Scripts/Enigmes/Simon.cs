using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameState;

public class Simon : MonoBehaviour
{

    // list de niveaux ou on peut choisir l'ordre des couleurs en auto en amont dans l'editeur, du coup une struct niveau1, list de couleurs
    // Enum avec les couleurs
    // faire défiler dans l'ordre les couleurs avec un gros temps de pause régulable
    // centre de controle click sur les couleurs dans l'ordre
    // faire vérification de l'ordre, on peut les ajouter dans un stack
    // reset lorsqu'ils ont faux, faut vérifier la taille du ColorOrder



    //demander a Nico lorsqu'on fait une faute

    public enum SimonColor
    {
        GREEN = 0,
        RED, 
        BLUE,
        YELLOW
    }

    [Serializable]
    struct SimonLevel
    {
        public int Level;
        public List<SimonColor> ColorOrder;
    }

    [Serializable]
    struct SimonLight
    {
        public SimonColor Color;
        public Light Light;
    }

    [SerializeField] private List<SimonLevel> _levelList = new List<SimonLevel>();
    public int _currentLevel = 0;
    [SerializeField] private List<SimonColor> _colorsStackEnteredByPlayer = new List<SimonColor>();
    [SerializeField] private List<SimonLight> _colorsLights = new List<SimonLight>(4);

    private Coroutine _colorRoutine = null;
    private float _holdColorTime = 2f;
    private float _pauseBlankTime = 3f;

    private bool _canChooseColor = true;

    private void Start()
    {
        _colorsStackEnteredByPlayer.Clear();
        StartFirstSimonLevel();
    }

    public void PushButton(string color)
    {
        if (!_canChooseColor)
            return;

        foreach (var enumValue in Enum.GetValues(typeof(SimonColor)))
        {
            if (enumValue.ToString() == color.ToUpper())
            {
                _colorsStackEnteredByPlayer.Add((SimonColor)enumValue);
            }
        }

        if (isColorOrderFinished)
            ColorsOrderVerification();

    }

    private bool isColorOrderFinished => _colorsStackEnteredByPlayer.Count == _levelList[_currentLevel].ColorOrder.Count;

    private void ColorsOrderVerification()
    {
        bool isOrderCorrect = true;
        for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
        {
            if (_colorsStackEnteredByPlayer[i] != _levelList[_currentLevel].ColorOrder[i])
            {
                isOrderCorrect = false;
                break;
            }
        }

        Debug.Log(isOrderCorrect ? "Order correct!" : "Order is not good!");

        if (isOrderCorrect && _currentLevel < (_levelList.Count - 1))
        {
            DisableAllLights();
            if(_colorRoutine != null)
                StopCoroutine(_colorRoutine);
            _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel++));
            Debug.Log("Next level is " + _currentLevel);
        } 
        else if (isOrderCorrect && _currentLevel >= (_levelList.Count - 1))
        {
            _canChooseColor = false;
            DisableAllLights();
            if (_colorRoutine != null)
                StopCoroutine(_colorRoutine);
            Debug.Log("It was the last level");
        }

        //_canChooseColor = currentLevel < (_levelList.Count - 1);

        _colorsStackEnteredByPlayer.Clear();

    }

    public void StartFirstSimonLevel()
    {
        _colorRoutine = StartCoroutine(DisplayColorRoutine(_currentLevel));
    }

    private IEnumerator DisplayColorRoutine(int level)
    {
        while (_canChooseColor)
        {
            for (int i = 0; i < _levelList[_currentLevel].ColorOrder.Count; i++)
            {
                Light lightToEnable = ChooseLight(_levelList[_currentLevel].ColorOrder[i]);
                lightToEnable.enabled = true;
                yield return new WaitForSeconds(_holdColorTime);
                lightToEnable.enabled = false;
            }
            yield return new WaitForSeconds(_pauseBlankTime);
            yield return null;
        }
        
    }

    private void DisableAllLights()
    {
        foreach (var item in _colorsLights)
        {
            item.Light.enabled = false;
        }
    }

    private Light ChooseLight(SimonColor simon) => _colorsLights.First(color => color.Color.Equals(simon)).Light;
}
