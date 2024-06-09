using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.XR.Content.Interaction;

public class Launch : NetworkBehaviour, IGameState
{
    private bool _canAttach;
    private bool _canPushButton;
    public bool CanAttach { get => _canAttach; set => _canAttach = value; }
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;

    [SerializeField] private VideoPlayer _launchVideo;

    private NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Countdown")]
    [SerializeField, Range(0, 30)] private float _countdownBeforeButton;
    [SerializeField] private TextMeshProUGUI _textCountdown;
    [SerializeField] private GameObject _layoutPassword;
    [ReadOnly] public float _currentCountdown;
    private Coroutine countdownRoutine;

    [Header("Ceintures")]
    [SerializeField] private GameObject _ceintureOpen;
    [SerializeField] private GameObject _ceintureClosed;

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnStateComplete;

    private void Reset()
    {
        _canAttach = false;
        _countdownBeforeButton = 10f;
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

        CountdownButtonRpc();
    }
    public void DetachPlayer()
    {
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
        _ceintureOpen.SetActive(true);
        _ceintureClosed.SetActive(false);
    }


    [Rpc(SendTo.Everyone)]
    private void CountdownButtonRpc()
    {
        Debug.Log("CountdownButtonRpc");
        if (countdownRoutine == null)
            countdownRoutine = StartCoroutine(CountdownButton());
    }

    private IEnumerator CountdownButton()
    {
        PlayVoiceOffClientRpc(0);
        yield return new WaitForSeconds(5); // wait for voiceOFF
        _currentCountdown = _countdownBeforeButton;
        _textCountdown.gameObject.SetActive(true);
        while (_currentCountdown > 0)
        {
            Debug.Log(_currentCountdown); 
            _textCountdown.text = _currentCountdown.ToString();
            _currentCountdown--;
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

    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        if (_playerIsLock.Value)
        {
            PlayVideoRpc();
            GameState.Instance.ChangeState(GameState.GAMESTATES.VALVES);
            DetachPlayer();
            OnStateComplete?.Invoke();
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
    public void StartState()
    {
        OnStateStart?.Invoke();
        _canAttach = true;
        _layoutPassword.SetActive(false);
    }


}
