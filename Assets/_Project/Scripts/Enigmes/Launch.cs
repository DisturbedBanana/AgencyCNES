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
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;
    [SerializeField, Range(0, 30)] private float _timeBeforeDetach;

    private NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private void Reset()
    {
        _canAttach = false;
        _timeBeforeDetach = 5f;
    }

    public void AttachPlayer()
    {
        if (!CanAttach)
            return;

        _playerController.GetComponent<Collider>().enabled = false;
        _playerController.LockMovement(true);
        _playerController.transform.position = _attach.position;
        _playerController.transform.rotation = _attach.rotation;
        if(IsOwner)
            _playerIsLock.Value = true;
    }
    public void DetachPlayer()
    {
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
    }

    private IEnumerator WaitBeforeDetach()
    {
        yield return new WaitForSeconds(_timeBeforeDetach);
        DetachPlayer();
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
                GameState.instance.ChangeState(GameState.GAMESTATES.VALVES);
                StartCoroutine(WaitBeforeDetach());
                _canAttach = false;
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
    }


}
