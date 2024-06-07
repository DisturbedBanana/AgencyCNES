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
    private bool _canPushButton;
    public bool CanAttach { get => _canAttach; set => _canAttach = value; }
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;

    [SerializeField, Range(0, 30)] private float _timeCountdown;
    [SerializeField] private VideoPlayer _launchVideo;

    private NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [ReadOnly] public float _countdown;

    [Header("Ceintures")]
    [SerializeField] private GameObject _ceintureOpen;
    [SerializeField] private GameObject _ceintureClosed;

    [Header("Events")]
    public UnityEvent OnComplete;

    private void Reset()
    {
        _canAttach = false;
        _timeCountdown = 10f;
    }

    private void Start()
    {
        _canAttach = false;
        _canPushButton = false;
        countdownRoutine = null;
        _ceintureOpen.SetActive(true);
        _ceintureClosed.SetActive(false);
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


        _ceintureOpen.SetActive(false);
        _ceintureClosed.SetActive(true);
    }
    public void DetachPlayer()
    {
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
        _ceintureOpen.SetActive(true);
        _ceintureClosed.SetActive(false);
    }


    public void LaunchCountdownForSitting()
    {
        CountdownRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        if (_playerIsLock.Value)
        {
            PlayVideoRpc();
            GameState.Instance.ChangeState(GameState.GAMESTATES.VALVES);
            DetachPlayer();
            OnComplete?.Invoke();
        }
        else
        {
            PlayVoiceOffClientRpc(1);
        }
        _canAttach = false;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayVideoRpc()
    {
        _launchVideo.gameObject.SetActive(true);
        _launchVideo.Play();
    }



    [Rpc(SendTo.Everyone)]
    private void CountdownRpc()
    {
        Debug.Log("CountdownRpc");
        _canAttach = true;

        if (countdownRoutine == null)
            countdownRoutine = StartCoroutine(Countdown());
    }
    Coroutine countdownRoutine;
    private IEnumerator Countdown()
    {
        Debug.Log("Countdown routine");
        PlayVoiceOffClientRpc(0);
        _countdown = _timeCountdown;
        while(_countdown > 0)
        {
            if (_playerIsLock.Value)
            {
                Debug.Log("Player is lock");
                break;
            }
            Debug.Log(_countdown);
            _countdown--;
            yield return new WaitForSeconds(1);
        }

        if (NetworkManager.Singleton.IsHost)
            CheckPlayerLockServerRpc();

        countdownRoutine = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckPlayerLockServerRpc()
    {
        if (_playerIsLock.Value)
        {
            CountdownButtonRpc();
        }
        else
        {
            PlayVoiceOffClientRpc(1);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void CountdownButtonRpc()
    {
        Debug.Log("CountdownButtonRpc");
        _canPushButton = true;
        if(countdownRoutine == null)
            countdownRoutine = StartCoroutine(CountdownButton());
    }

    private IEnumerator CountdownButton()
    {
        PlayVoiceOffClientRpc(0);
        _countdown = 10;
        while (_countdown > 0)
        {
            Debug.Log(_countdown);
            _countdown--;
            yield return new WaitForSeconds(1);
        }

        _canPushButton = true;

        countdownRoutine = null;
    }


    public void PlayerPushedButton()
    {
        if (GameState.Instance.CurrentGameState != GameState.GAMESTATES.LAUNCH || !_playerIsLock.Value || !_canPushButton)
            return;

        CheckLaunchServerRpc();
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
