using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class Launch : NetworkBehaviour
{
    [SerializeField] private bool _canAttach;
    public bool CanAttach { get => _canAttach; set => _canAttach = value; }
    [SerializeField] private XRLockSocketInteractor _socketInteractor;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;

    [SerializeField] NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private void Reset()
    {
        _canAttach = true;
    }

    public void AttachPlayer()
    {
        if (!CanAttach)
            return;
        Debug.Log("PlayerAttach");
        _playerController.GetComponent<Collider>().enabled = false;
        _playerController.LockMovement(true);
        _playerController.transform.position = _attach.position;
        _playerController.transform.rotation = _attach.rotation;
        if(IsOwner)
            _playerIsLock.Value = true;
    }
    public void DetachPlayer()
    {
        Debug.Log("PlayerDetach");
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
        _playerController.GetComponent<Rigidbody>().AddForce(Vector3.forward, ForceMode.Impulse);
    }

    public void PlayerPushedButton()
    {
        if (!_playerIsLock.Value)
            return;
        CheckLaunchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        try
        {
            if (_playerIsLock.Value)
            {
                //Launch
                GameState.instance.ChangeState(GameState.GAMESTATES.SIMONSAYS);
                DetachPlayer();
                _canAttach = false;
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
    }


}
