using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Frequency : NetworkBehaviour
{
    [Header("Line renderer")]
    [SerializeField] private LineRenderer _myLineRenderer;
    [SerializeField, Range(0.001f, 0.02f)] private float _lineWidth;
    [SerializeField, Range(100, 300)] private int _pointsMultiplicator;
    [SerializeField] private Vector2 _xLimits;
    [SerializeField, Range(0.5f, 100f)] private float _movementSpeed;
    [SerializeField, Range(0, 2 * Mathf.PI)] private float _radians;

    [Header("Amplitude")]
    [InfoBox("Don't put amplitude above 1", EInfoBoxType.Normal)]
    [SerializeField] private NetworkVariable<float> _amplitude = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField, Range(0,1)] private float _maxAmplitude;
    [SerializeField, Range(0, 1)] private float _minAmplitude;

    [Header("Frequence")]
    [InfoBox("Don't put frequence above 1", EInfoBoxType.Normal)]
    [SerializeField] private NetworkVariable<float> _frequency = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField, Range(0, 1)] private float _maxFrequency;
    [SerializeField, Range(0, 1)] private float _minFrequency;

    [Header("Target Objectif")]
    [SerializeField] private LineRenderer _targetLineRenderer;
    [SerializeField] private Color _targetColor;
    [SerializeField, Range(0.01f, 10f)] private float _targetDifference;
    [SerializeField, Range(0, 1)] private float _targetFrequency;
    [SerializeField, Range(0, 1)] private float _targetAmplitude;
    [SerializeField] private NetworkVariable<bool> _isTargetFrequency = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsTargetFrequency { get => _isTargetFrequency; set => _isTargetFrequency = value; }

    [Header("Events")]
    public UnityEvent OnCorrectTargetValue;

    private void Reset()
    {
        _myLineRenderer = GetComponent<LineRenderer>();
        _lineWidth = 0.005f;
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;
        _pointsMultiplicator = 150;

        _maxAmplitude = 1f;
        _minAmplitude = 0.1f;

        _maxFrequency = 1f;
        _minFrequency = 0.1f;

        _xLimits = new Vector2(0, 8);
        _movementSpeed = 1;
        _targetDifference = 0.1f;
    }

    private void Start()
    {
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;

        _targetLineRenderer.startWidth = _targetLineRenderer.endWidth = _lineWidth;
        _targetLineRenderer.startColor = _targetLineRenderer.endColor = _targetColor;
    }

    private void Update()
    {
        Draw();
        DrawTargetLine();
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
    private void DrawTargetLine()
    {
        float xStart = _xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = _xLimits.y;

        _targetLineRenderer.positionCount = _pointsMultiplicator;
        for (int currentPoint = 0; currentPoint < _targetLineRenderer.positionCount; currentPoint++)
        {
            float progress = (float)currentPoint / (_targetLineRenderer.positionCount - 1);
            float x = Mathf.Lerp(xStart, xFinish, progress);
            float y = _targetAmplitude * Mathf.Sin((Tau * _targetFrequency * x) + (Time.timeSinceLevelLoad * _movementSpeed)) ;
            Vector3 newBeginPos = transform.localToWorldMatrix * new Vector4(x, y, 0, 1);

            _targetLineRenderer.SetPosition(currentPoint, newBeginPos);

        }
    }

    private void CheckTargetValues()
    {
        if (GameState.Instance.CurrentGameState != GameState.GAMESTATES.FREQUENCY)
            return;

        if(Mathf.Abs(_targetFrequency - _frequency.Value) <= _targetDifference && Mathf.Abs(_targetAmplitude - _amplitude.Value) <= _targetDifference)
        {
            if (_isTargetFrequency.Value != true)
                ChangeTargetValueServerRpc(true);
        }
        else
        {
            if (_isTargetFrequency.Value != false)
                ChangeTargetValueServerRpc(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeTargetValueServerRpc(bool value)
    {
        _isTargetFrequency.Value = value;
        ChangeLineColorClientRpc(value);
        if (value && transform.parent.TryGetComponent(out FrequenciesCheck fcheck))
        {
            fcheck.CheckFrequenciesServerRpc();
        } 

    }

    [ClientRpc]
    private void ChangeLineColorClientRpc(bool isTarget)
    {
        _myLineRenderer.startColor = _myLineRenderer.endColor = isTarget ? Color.green : Color.white;

        if (isTarget && _myLineRenderer.startColor != Color.green)
            OnCorrectTargetValue?.Invoke();
    }

    #region Amplitude
    public void ChangeAmplitude(float value)
    {
        if (value <= _maxAmplitude && value >= _minAmplitude)
        {
            ChangeAmplitudeServerRpc(value);
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
        if (value <= _maxFrequency && value > _minFrequency)
        {
            ChangeFrequencyServerRpc(value);
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


}
