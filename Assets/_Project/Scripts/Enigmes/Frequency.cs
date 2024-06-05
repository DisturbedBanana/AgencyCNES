using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Frequency : NetworkBehaviour
{
    [Header("Line renderer")]
    [SerializeField] private LineRenderer _myLineRenderer;
    [SerializeField, Range(0.001f, 0.02f)] private float _lineWidth;

    [SerializeField] private int _pointsMultiplicator;

    [Header("Sensitivity")]
    [SerializeField] private float _sensitivity;
    [SerializeField] private float _sensitivitySteps;
    [SerializeField] private float _minSensitivity;
    [SerializeField] private float _maxSensitivity;

    [Header("Amplitude")]
    [SerializeField] private NetworkVariable<float> _amplitude = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //[SerializeField] private float _amplitude;
    [SerializeField] private float _maxAmplitude;
    [SerializeField] private float _minAmplitude;

    [Header("Frequence")]
    [SerializeField] private NetworkVariable<float> _frequency = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //[SerializeField] private float _frequency;
    [SerializeField] private float _maxFrequency;
    [SerializeField] private float _minFrequency;

    [SerializeField] private Vector2 _xLimits;
    [SerializeField] private float _movementSpeed;
    [SerializeField, Range(0, 2 * Mathf.PI)] private float _radians;

    [Header("Objectif Validation")]
    [SerializeField] private float _targetDifference;
    [SerializeField] private float _targetFrequency;
    [SerializeField] private float _targetAmplitude;
    [SerializeField] private NetworkVariable<bool> _isTargetFrequency = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsTargetFrequency { get => _isTargetFrequency; set => _isTargetFrequency = value; }

    private void Reset()
    {
        _myLineRenderer = GetComponent<LineRenderer>();
        _lineWidth = 0.005f;
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;
        _pointsMultiplicator = 150;

        _maxAmplitude = 1f;
        _maxFrequency = 1f;
        _xLimits = new Vector2(0, 8);
        _movementSpeed = 1;
        _sensitivity = 1f;
        _sensitivitySteps = 0.1f;
        _minSensitivity = 0.1f;
        _maxSensitivity = 2f;
        _targetDifference = 0.1f;
    }

    private void Start()
    {
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;
    }

    private void Update()
    {
        Draw();
    }
    private void Draw()
    {
        float xStart = _xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = _xLimits.y;

        _myLineRenderer.positionCount = _pointsMultiplicator;
        for (int currentPoint = 0; currentPoint < _myLineRenderer.positionCount; currentPoint++)
        {
            float progress = (float)currentPoint / (_myLineRenderer.positionCount - 1);
            float x = Mathf.Lerp(xStart, xFinish, progress);
            float y = _amplitude.Value * Mathf.Sin((Tau * _frequency.Value * x) + (Time.timeSinceLevelLoad * _movementSpeed)) ;
            Vector3 newBeginPos = transform.localToWorldMatrix * new Vector4(x, y, 0, 1);

            _myLineRenderer.SetPosition(currentPoint, newBeginPos);

        }
    }

    private void CheckTargetValues()
    {
        if (GameState.instance.CurrentGameState != GameState.GAMESTATES.FREQUENCY)
            return;

        if(Mathf.Abs(_targetFrequency - _frequency.Value) <= _targetDifference && Mathf.Abs(_targetAmplitude - _amplitude.Value) <= _targetDifference)
        {
            _myLineRenderer.startColor = _myLineRenderer.endColor = Color.green;

            if (_isTargetFrequency.Value != true)
                ChangeNetworkVariableValueServerRpc(true);
        }
        else
        {
            _myLineRenderer.startColor = _myLineRenderer.endColor = Color.white;

            if (_isTargetFrequency.Value != false)
                ChangeNetworkVariableValueServerRpc(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeNetworkVariableValueServerRpc(bool value)
    {
        _isTargetFrequency.Value = value;
        if(value && transform.parent.TryGetComponent(out FrequenciesCheck fcheck))
        {
            fcheck.CheckFrequenciesServerRpc();
        } 

    }

    #region Amplitude
    public void ChangeAmplitude(float value)
    {
        if (value > _amplitude.Value && (_amplitude.Value + GetSensitivity()) < _maxAmplitude)
        {
            ChangeAmplitudeServerRpc(_amplitude.Value + GetSensitivity());
        }
        else if (value < _amplitude.Value && (_amplitude.Value - GetSensitivity()) > _minAmplitude)
        {
            ChangeAmplitudeServerRpc(_amplitude.Value - GetSensitivity());
        }
        CheckTargetValues();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeAmplitudeServerRpc(float amplitude)
    {
        _amplitude.Value = amplitude;
    }
    #endregion

    #region Frequency
    public void ChangeFrequency(float value)
    {
        if (value > _frequency.Value && (_frequency.Value + GetSensitivity()) < _maxFrequency)
        {
            ChangeFrequencyServerRpc(_frequency.Value + GetSensitivity());
        }
        else if (value < _frequency.Value && (_frequency.Value - GetSensitivity()) > _minFrequency)
        {
            ChangeFrequencyServerRpc(_frequency.Value - GetSensitivity());
        }
        CheckTargetValues();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeFrequencyServerRpc(float frequency)
    {
        _frequency.Value = frequency;
    }
    #endregion

    #region Joystick
    public void ChangeAmplitudeJoystick(float value)
    {
        if (value == 0)
            return;
        ChangeAmplitude(ConvertRangeJoystick(value));
    }

    public void ChangeFrequencyJoystick(float value)
    {
        if (value == 0)
            return;
        ChangeFrequency(ConvertRangeJoystick(value));
    }
    private float ConvertRangeJoystick(float originalValue) => (originalValue + 1) / 2;
    #endregion

    #region Sensitivity
    private float GetSensitivity() => _sensitivity / 100;

    public void IncreaseSensitivity()
    {
        if (_sensitivity >= _maxSensitivity)
            return;

        _sensitivity += _sensitivitySteps;
    }
    public void DecreaseSensitivity()
    {
        if (_sensitivity <= _minSensitivity)
            return;

        _sensitivity -= _sensitivitySteps;
    }
    #endregion

}
