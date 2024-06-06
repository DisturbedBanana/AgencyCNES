using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

[Serializable]
public struct PlayerSpawn
{
    [SerializeField] private PlayerController.MOVEMENTTYPE _movementType;
    [SerializeField] private Transform _spawnTransform;
    public PlayerController.MOVEMENTTYPE MovementType { get => _movementType; set => _movementType = value; }
    public Transform SpawnTransform { get => _spawnTransform; set => _spawnTransform = value; }
}

public class PlayerController : CharacterControllerDriver
{
    public enum MOVEMENTTYPE
    {
        LAUNCHER = 0,
        MISSIONCONTROL
    }

    

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField, Range(0, 4)] private float _maxMagnitude;
    [SerializeField, Range(0, 10)] private float _decreaseSpeed;

    [SerializeField, ReadOnly] private float _currentMagnitude;
    [SerializeField] private GameObject _locomotion;

    private MOVEMENTTYPE _movementType;

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _maxMagnitude = 1; 
        _decreaseSpeed = 1;
    }

    public void SpawnPlayer(PlayerSpawn playerSpawn)
    {
        ChangeMovementType(playerSpawn.MovementType);
        TeleportToSpawnPosition(playerSpawn.SpawnTransform.position);
    }

    public void ChangeMovementType(MOVEMENTTYPE movementType)
    {
        switch (movementType)
        {
            case MOVEMENTTYPE.LAUNCHER:
                _rigidbody.useGravity = false;
                break;
            case MOVEMENTTYPE.MISSIONCONTROL:
                _rigidbody.useGravity = true;
                break;
        }
    }

    public void TeleportToSpawnPosition(Vector3 pos)
    {
        transform.parent.position = pos;
    }

    protected new void Start(){ } // Don't delete

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

    public void LockMovement(bool enable)
    {
        _locomotion.SetActive(!enable);
        if (enable)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
            _rigidbody.freezeRotation = true;
        }
    }
}
