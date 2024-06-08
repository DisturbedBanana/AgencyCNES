using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ValvePuzzlePart : NetworkBehaviour
{
    [Header("Variables")]
    [SerializeField] float _minValue;
    [SerializeField] float _maxValue;
    [SerializeField] float _currentValue;
    [SerializeField] float _targetValue;
    [SerializeField] float _correctValue;
    [SerializeField, Range(50, 200)] float _speed;
    [SerializeField] bool _isSolved;
    [SerializeField, Range(0.05f, 10f)] private float _targetDifference;

    [Header("References")]
    [SerializeField] Transform _needle;
    [SerializeField] XRKnob _knob;


    NetworkVariable<float> _angle = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Events")]
    public UnityEvent OnCorrectAngle;
    public UnityEvent OnCorrectAngleChangedToIncorrect;

    public float CurrentValue
    {
        get { return _currentValue; }
        set { _currentValue = value; }
    }

    public float TargetValue
    {
        get { return _targetValue; }
        set { _targetValue = value; }
    }

    public bool IsSolved
    {
        set { _isSolved = value; }
        get { return _isSolved; }

    }

    private void Reset()
    {
        _speed = 50;
        _minValue = -90;
        _maxValue = 90;
        _targetDifference = 1;
        _isSolved = false;
    }

    private void Start()
    {
        //RollRandomValues();
        _needle.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        _currentValue = _targetValue;
        _isSolved = false;
    }

    private void Update()
    {
        if (_currentValue != _angle.Value)
        {
            _currentValue = Mathf.MoveTowards(_currentValue, _angle.Value, _speed * Time.deltaTime);
            _needle.localEulerAngles = new Vector3(Mathf.Lerp(-90, 90, (_currentValue - _minValue) / (_maxValue - _minValue)), 0, 0);
        }

        if (GameState.Instance.CurrentGameState != GameState.GAMESTATES.VALVES)
            return;
           
        if (IsCorrectValue())
        {
            _needle.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
        else
        {
            _needle.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }

    private void RollRandomValues()
    {
        _currentValue = UnityEngine.Random.Range(_minValue, _maxValue);
        _correctValue = UnityEngine.Random.Range(_minValue, _maxValue);

        if (IsCorrectValue())
        {
            RollRandomValues();
        }
    }

    private bool AreValuesRoughlyEqual()
    {
        return Mathf.Abs(_currentValue - _correctValue) <= (_maxValue - _minValue) * 0.01f;
    }

    private bool IsCorrectValue() => Mathf.Abs(_correctValue - _currentValue) <= _targetDifference;

    public void ChangeTargetValue()
    {
        _angle.Value = _knob.value;

    }

    
    public void LetGoOfHandle() 
    {
        if (GameState.Instance.CurrentGameState != GameState.GAMESTATES.VALVES)
            return;

        if (IsCorrectValue() && !_isSolved) //Changed to Correct
            OnCorrectAngle?.Invoke();
        else if (!IsCorrectValue() && _isSolved)
            OnCorrectAngleChangedToIncorrect?.Invoke(); //Changed from Correct to Incorrect


        if (IsCorrectValue())
            _isSolved = true;
        else
            _isSolved = false;

        ValveManager.instance.CheckValves();
    }
}


