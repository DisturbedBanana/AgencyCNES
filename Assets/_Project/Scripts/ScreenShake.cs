using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private Slider _intensitySlider;
    [SerializeField] private Slider _timeSlider;
    private Cinemachine.CinemachineImpulseSource _impulseSource;

    private void Start()
    {
        _impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
    }

    public void Shake()
    {
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_AttackTime = _timeSlider.value;
        _impulseSource.GenerateImpulse(_intensitySlider.value);
    }
}
