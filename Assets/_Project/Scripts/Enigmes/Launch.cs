using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    private int _currentHintIndex = 0;

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
    public UnityEvent OnPlayerAttached;
    public UnityEvent OnCountdownFinished;
    public UnityEvent OnPlayerDettached;
    public UnityEvent<VoiceAI> OnHint;
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


    public void StartState()
    {
        //OnStateStart?.Invoke();
        _canAttach = true;
        _currentHintIndex = 0;
        _layoutPassword.SetActive(false);
        PlayVoiceOffClientRpc(2);
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
    }


    private IEnumerator StartHintCountdown()
    {
        if (_voicesHint.Count == 0)
            yield break;

        for (int i = 0; i < _voicesHint[_currentHintIndex].numberOfRepeat + 1; i++)
        {
            float waitBeforeHint = _voicesHint[_currentHintIndex].delayedTime;
            int waitingHintIndex = _currentHintIndex;

            while (waitBeforeHint > 0)
            {
                if (waitingHintIndex != _currentHintIndex)
                {
                    waitingHintIndex = _currentHintIndex;
                    waitBeforeHint = _voicesHint[_currentHintIndex].delayedTime;
                }

                waitBeforeHint -= Time.deltaTime;
                yield return null;
            }
            SoundManager.Instance.PlaySound(gameObject, _voicesHint[_currentHintIndex].audio);

        }


        if (_currentHintIndex < _voicesHint.Count - 1)
        {
            _currentHintIndex++;
            StartCoroutine(StartHintCountdown());
        }
    }



    public void AttachPlayer()
    {
        if (!_canAttach)
            return;

        _playerController.GetComponent<Collider>().enabled = false;
        _playerController.LockMovement(true);
        _playerController.transform.position = _attach.position;
        _playerController.transform.rotation = _attach.rotation;

        if (IsOwner)
            _playerIsLock.Value = true;


        _ceintureOpen.SetActive(false);
        _ceintureClosed.SetActive(true);

        OnPlayerAttached?.Invoke();
        CountdownButtonRpc();
    }
    public void DetachPlayer()
    {
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
        _ceintureOpen.SetActive(true);
        _ceintureClosed.SetActive(false);
        OnPlayerDettached?.Invoke();
    }


    [Rpc(SendTo.Everyone)]
    private void CountdownButtonRpc()
    {
        StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownButton());
    }

    private IEnumerator CountdownButton()
    {
        //AudioClip audioClip = Resources.Load<AudioClip>("Audio/Countdown");
        PlayVoiceOffClientRpc(0);
        //yield return new WaitForSeconds(audioClip.length); // wait for voiceOFF
        _currentCountdown = _countdownBeforeButton;
        _textCountdown.gameObject.SetActive(true);
        var oneSecondWait = new WaitForSeconds(1);
        while (_currentCountdown > 0)
        {
            Debug.Log(_currentCountdown);
            _currentCountdown--;
            _textCountdown.text = _currentCountdown.ToString();
            yield return oneSecondWait;
        }
        OnCountdownFinished?.Invoke();
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
                Debug.Log("Starting Countdown");
                break;
            case 1:
                Debug.Log($"Player not attached. Please restart");
                break;
            case 2:
                Debug.Log($"Player Need to attach");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        if (_playerIsLock.Value)
        {
            EndStateClientRpc();
        }
        else
        {
            PlayVoiceOffClientRpc(1);
        }
    }

    [ClientRpc]
    private void EndStateClientRpc()
    {
        StopAllCoroutines();
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllEndVoices());
        _textCountdown.gameObject.SetActive(false);
        _canAttach = false;
        PlayVideoRpc();
        DetachPlayer();
        OnStateComplete?.Invoke();
        if (IsOwner)
            GameState.Instance.ChangeState(GameState.GAMESTATES.VALVES);
    }

    private void PlayVideoRpc()
    {
        _launchVideo.gameObject.SetActive(true);
        _launchVideo.Play();
    }


}
