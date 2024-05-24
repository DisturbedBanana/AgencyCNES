using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GaugePuzzle : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float _minValue;
    [SerializeField] float _maxValue;
    [SerializeField] float _currentValue;
    [SerializeField] float _targetValue;
    [SerializeField] float _speed = 1;
    [SerializeField] float _tolerance;
    [SerializeField] float _toleranceTime;
    [SerializeField] bool _isSolved;

    [Header("References")]
    [SerializeField] Transform _needle;
    [SerializeField] GameObject _valve;

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
        get { return _isSolved; }
    }

    private void Start()
    {
        _currentValue = Random.Range(_minValue, _maxValue);
        _targetValue = Random.Range(_minValue, _maxValue);
    }

    private void Update()
    {
        if (_currentValue != _targetValue)
        {
            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, _speed * Time.deltaTime);
            _needle.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(-90, 90, (_currentValue - _minValue) / (_maxValue - _minValue)));
        }
    }

    public void ChangeGaugeValue()
    {
        _targetValue = _valve.GetComponent<XRKnob>().value;

    }
}
