using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.XR.Content.Interaction;

public class Launch : NetworkBehaviour
{
    private bool _canAttach;
    public bool CanAttach { get => _canAttach; set => _canAttach = value; }
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;

    [SerializeField, Range(0, 30)] private float _timeCountdown;
    [SerializeField] private VideoPlayer _launchVideo;

    private NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField, ReadOnly] private float _countdown;

    [Header("Events")]
    public UnityEvent OnComplete;

    private void Reset()
    {
        _canAttach = false;
        _timeCountdown = 10f;
    }

    public void AttachPlayer()
    {
        if (!_canAttach)
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


    public void PlayerPushedButton()
    {
        if (GameState.instance.CurrentGameState != GameState.GAMESTATES.LAUNCH)
            return;

        CountdownClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        if (_playerIsLock.Value)
        {
            PlayVideoClientRpc();
            GameState.instance.ChangeState(GameState.GAMESTATES.VALVES);
            DetachPlayer();
            OnComplete?.Invoke();
        }
        else
        {
            PlayVoiceOffClientRpc(1);
        }
        _canAttach = false;
    }

    [ClientRpc]
    private void PlayVideoClientRpc()
    {
        _launchVideo.gameObject.SetActive(true);
        _launchVideo.Play();
    }



    [ClientRpc]
    private void CountdownClientRpc()
    {
        _canAttach = true;

        if(countdownRoutine != null)
            countdownRoutine = Countdown();
    }
    IEnumerator countdownRoutine;
    private IEnumerator Countdown()
    {
        PlayVoiceOffClientRpc(0);
        _countdown = _timeCountdown;
        while(_countdown > 0)
        {
            _countdown -= Time.deltaTime;
            yield return null;
        }

        if (NetworkManager.Singleton.IsHost)
            CheckLaunchServerRpc();

        countdownRoutine = null; 
    }

    [ClientRpc]
    private void PlayVoiceOffClientRpc(int value)
    {
        switch (value)
        {
            case 0:
                Debug.Log("Countdown");
                break;
            case 1:
                Debug.Log($"Player not attached. Please restart"); 
                break;
        }
    }
}
