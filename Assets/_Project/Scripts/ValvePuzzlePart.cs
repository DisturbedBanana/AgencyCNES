using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ValvePuzzlePart : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float _minValue;
    [SerializeField] float _maxValue;
    [SerializeField] float _currentValue;
    [SerializeField] float _targetValue;
    [SerializeField] float _correctValue;
    [SerializeField] float _speed = 1;
    [SerializeField] bool _isSolved;

    [Header("References")]
    [SerializeField] Transform _needle;
    [SerializeField] XRKnob _knob;

    Coroutine _toleranceCoroutine = null;

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

    private void Start()
    {
        RollRandomValues();
    }

    private void Update()
    {
        if (_currentValue != _targetValue)
        {
            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, _speed * Time.deltaTime);
            _needle.localEulerAngles = new Vector3(Mathf.Lerp(-90, 90, (_currentValue - _minValue) / (_maxValue - _minValue)), 0, 0);
        }

        if (AreValuesRoughlyEqual())
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

        if (AreValuesRoughlyEqual())
        {
            RollRandomValues();
        }
    }

    private bool AreValuesRoughlyEqual()
    {
        //return CurrentValue - _correctValue <= Mathf.Abs(10);
        return CurrentValue - _correctValue < 10 && CurrentValue - _correctValue > 10;
    }

    public void ChangeTargetValue()
    {
        TargetValue = _knob.value;
    }

    public void LetGoOfHandle() 
    {
        if (AreValuesRoughlyEqual())
        {
            IsSolved = true;
        }
    }
}


