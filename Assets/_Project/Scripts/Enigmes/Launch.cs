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

public class Launch : NetworkBehaviour, IGameState, IVoiceAI
{
    private bool _canAttach;
    private bool _canPushButton;
    public bool CanAttach { get => _canAttach; set => _canAttach = value; }
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _attach;

    [SerializeField] private VideoPlayer _launchVideo;


    [Header("Voices")]
    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    private NetworkVariable<int> _currentHintIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Countdown")]
    [SerializeField, Range(0, 30)] private float _countdownBeforeButton;
    [SerializeField] private TextMeshProUGUI _textCountdown;
    [SerializeField] private GameObject _layoutPassword;
    [ReadOnly] public float _currentCountdown;
    private Coroutine countdownRoutine;

    [Header("Siège")]
    private NetworkVariable<bool> _playerIsLock = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private GameObject _ceintureOpen;
    [SerializeField] private GameObject _ceintureClosed;
    [SerializeField] private float _waitSecondsBeforeDetach;

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnPlayerAttached;
    public UnityEvent OnCountdownFinished;
    public UnityEvent OnPlayerDetached;
    public UnityEvent OnStateComplete;

    private void Reset()
    {
        _canAttach = false;
        _countdownBeforeButton = 10f;
        _waitSecondsBeforeDetach = 25f;
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
        OnStateStart?.Invoke();
        _canAttach = true;
        _layoutPassword.SetActive(false);
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
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

        OnPlayerAttachedClientRpc();
        CountdownButtonClientRpc();
    }

    [ClientRpc]
    private void OnPlayerAttachedClientRpc()
    {
        OnPlayerAttached?.Invoke();
    }

    [ClientRpc]
    private void OnPlayerDetachedClientRpc()
    {
        OnPlayerDetached?.Invoke();
    }


    [ClientRpc]
    private void CountdownButtonClientRpc()
    {
        if(countdownRoutine != null)
            StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownButton());
    }

    private IEnumerator CountdownButton()
    {
        _currentCountdown = _countdownBeforeButton;
        _textCountdown.gameObject.SetActive(true);
        var oneSecondWait = new WaitForSeconds(1);
        SoundManager.Instance.PlaySound(gameObject, _voicesAI.GetAllSpecialVoices()[0].audio);
        while (_currentCountdown > 0)
        {
            Debug.Log(_currentCountdown);
            _currentCountdown--;
            _textCountdown.text = _currentCountdown.ToString();
            yield return oneSecondWait;
        }
        OnCountdownFinishedClientRpc();
        _canPushButton = true;

        countdownRoutine = null;
    }

    [ClientRpc]
    public void OnCountdownFinishedClientRpc()
    {
        OnCountdownFinished?.Invoke();
    }


    public void PlayerPushedButton()
    {
        if (GameState.Instance.CurrentGameState != GameState.GAMESTATES.LAUNCH || !_playerIsLock.Value || !_canPushButton)
            return;

        CheckLaunchServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void CheckLaunchServerRpc()
    {
        if (_playerIsLock.Value)
        {
            OnStateCompleteClientRpc();
        }
    }

    [ClientRpc]
    public void OnStateCompleteClientRpc()
    {
        OnStateComplete?.Invoke();
        StopCoroutine(StartHintCountdown());
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllEndVoices());
        _textCountdown.gameObject.SetActive(false);
        _canAttach = false;
        PlayVideoRpc();
        WaitBeforeDetachPlayer();
        if (IsOwner)
            GameState.Instance.ChangeState(GameState.GAMESTATES.VALVES);
    }

    private void PlayVideoRpc()
    {
        _launchVideo.gameObject.SetActive(true);
        _launchVideo.Play();
    }

    private IEnumerator WaitBeforeDetachPlayer()
    {
        yield return new WaitForSeconds(_waitSecondsBeforeDetach);
        DetachPlayer();
    }

    public void DetachPlayer()
    {
        _playerController.GetComponent<Collider>().enabled = true;
        _playerController.LockMovement(false);
        _ceintureOpen.SetActive(true);
        _ceintureClosed.SetActive(false);
        OnPlayerDetachedClientRpc();
    }

    public IEnumerator StartHintCountdown()
    {
        if (_voicesHint.Count == 0)
            yield break;

        for (int i = 0; i < _voicesHint[_currentHintIndex.Value].numberOfRepeat + 1; i++)
        {
            float waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
            int waitingHintIndex = _currentHintIndex.Value;

            while (waitBeforeHint > 0)
            {
                if (waitingHintIndex != _currentHintIndex.Value)
                {
                    if (_currentHintIndex.Value > _voicesHint.Count - 1) yield break;
                    waitingHintIndex = _currentHintIndex.Value;
                    waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
                }

                waitBeforeHint -= Time.deltaTime;
                yield return null;
            }
            SoundManager.Instance.PlaySound(gameObject, _voicesHint[_currentHintIndex.Value].audio);
        }


        if (_currentHintIndex.Value < _voicesHint.Count - 1)
        {
            ChangeHintIndexServerRpc(_currentHintIndex.Value + 1);
            StartCoroutine(StartHintCountdown());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHintIndexServerRpc(int value)
    {
        _currentHintIndex.Value = value;
    }
}
