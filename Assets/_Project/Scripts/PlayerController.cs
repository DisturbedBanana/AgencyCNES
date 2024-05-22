using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

public class PlayerController : CharacterControllerDriver
{
    private Rigidbody _rigidbody;
    [SerializeField] private float _maxMagnitude;
    [SerializeField] private float _magnitude;

    // Start is called before the first frame update
    protected new void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_rigidbody == null)
            return;

        _magnitude = _rigidbody.velocity.magnitude;

        if (_rigidbody.velocity.magnitude > _maxMagnitude)
        {
            _rigidbody.velocity -= _rigidbody.velocity * Time.deltaTime;
        }
        
    }
}
