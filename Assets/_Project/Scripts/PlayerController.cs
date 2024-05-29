using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

public class PlayerController : CharacterControllerDriver
{
    private Rigidbody _rigidbody;
    [SerializeField, Range(0, 4)] private float _maxMagnitude;
    [SerializeField, Range(0, 10)] private float _decreaseSpeed;

    [SerializeField] private float _currentMagnitude;

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _maxMagnitude = 1; 
        _decreaseSpeed = 1;
    }

    protected new void Start()
    {

    }

    private void FixedUpdate()
    {
        if (_rigidbody == null)
            return;

        _currentMagnitude = _rigidbody.velocity.magnitude;

        if (_rigidbody.velocity.magnitude > _maxMagnitude)
        {
            _rigidbody.velocity -= _rigidbody.velocity * Time.fixedDeltaTime * _decreaseSpeed;
        }
    }
}
