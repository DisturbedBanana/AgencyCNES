using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Frequency : MonoBehaviour
{
    [Header("Line renderer")]
    [SerializeField] private LineRenderer _myLineRenderer;
    [SerializeField] private float _lineWidth;

    [SerializeField] private int _pointsMultiplicator;
    [SerializeField] private float _sensitivity;

    [Header("Amplitude")]
    [SerializeField] private float _amplitude;
    [SerializeField] private float _maxAmplitude;
    [SerializeField] private float _minAmplitude;

    [Header("Frequence")]
    [SerializeField] private float _frequency;
    [SerializeField] private float _maxFrequency;
    [SerializeField] private float _minFrequency;

    [SerializeField] private Vector2 _xLimits;
    [SerializeField] private float _movementSpeed;
    [SerializeField, Range(0, 2 * Mathf.PI)] private float _radians;

    private void Reset()
    {
        _myLineRenderer = GetComponent<LineRenderer>();
        _lineWidth = 0.1f;
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;
        _pointsMultiplicator = 150;

        _amplitude = _maxAmplitude = 1f;
        _frequency = _maxFrequency = 1f;
        _xLimits = new Vector2(0, 1);
        _movementSpeed = 1;
        _sensitivity = 0.01f;
    }

    private void Start()
    {
        _myLineRenderer.startWidth = _myLineRenderer.endWidth = _lineWidth;
    }

    public void ChangeAmplitude(float value)
    {
        if (value > _amplitude && (_amplitude + GetSensitivity()) < _maxAmplitude)
        {
            _amplitude += GetSensitivity(); 
        }
        else if (value < _amplitude && (_amplitude - GetSensitivity()) > _minAmplitude)
        {
            _amplitude -= GetSensitivity();
        }
    }

    public void ChangeFrequency(float value)
    {
        if (value > _frequency && (_frequency + GetSensitivity()) < _maxFrequency)
        {
            _frequency += GetSensitivity();
        }
        else if (value < _frequency && (_frequency - GetSensitivity()) > _minFrequency)
        {
            _frequency -= GetSensitivity();
        }
    }

    private float GetSensitivity() => _sensitivity / 100;


    void Draw()
    {
        float xStart = _xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = _xLimits.y;

        _myLineRenderer.positionCount = _pointsMultiplicator;
        for (int currentPoint = 0; currentPoint < _myLineRenderer.positionCount; currentPoint++)
        {
            float progress = (float)currentPoint / (_myLineRenderer.positionCount - 1);
            float x = Mathf.Lerp(xStart, xFinish, progress);
            float y = _amplitude * Mathf.Sin((Tau * _frequency * x) + (Time.timeSinceLevelLoad * _movementSpeed));
            Vector3 newBeginPos = transform.localToWorldMatrix * new Vector4(x, y, 0, 1);

            _myLineRenderer.SetPosition(currentPoint, newBeginPos);
            
        }
    }

    void Update()
    {
        Draw();
    }
}
