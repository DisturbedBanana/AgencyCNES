using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class PasswordPuzzleManager : NetworkBehaviour, IGameState
{
    bool _isCoroutineRunning = false;
    int _ignoreKeysAmount = 0;

    public enum PASSWORDKEYS
    {
        PASSWORDKEY1,
        PASSWORDKEY2,
        PASSWORDKEY3,
        PASSWORDKEY4,
        PASSWORDKEY5,
        PASSWORDKEY6,
        PASSWORDKEY7,
        PASSWORDKEY8,
        PASSWORDKEY9,
        PASSWORDKEY10,
        PASSWORDKEY11,
        PASSWORDKEY12
    }

    [Serializable]
    private struct PasswordSprites
    {
        public PASSWORDKEYS Key;
        public Sprite Sprite;
    }

    [Header("Password")]
    [SerializeField] List<PASSWORDKEYS> _correctPassword = new List<PASSWORDKEYS>();
    public List<PASSWORDKEYS> _currentPassword = new List<PASSWORDKEYS>();
    [SerializeField, Range(0, 5f)] private float _clearPasswordAfter;

    [Header("Keyboard")]
    [SerializeField] GameObject _keyboardToActivate;
    [SerializeField] GameObject _keyboardToDeactivate;

    [Header("Light")]
    [SerializeField] Light _light;
    [SerializeField, Range(2f, 15f)] float _lightFlashingSpeed = 10f;

    [Header("Computer Sprites")]
    [SerializeField] List<PasswordSprites> passwordsSprites = new List<PasswordSprites>(12);
    [SerializeField] Transform _computerSpritesParent;
    [SerializeField] Sprite _spriteBarre;

    [Header("Voices")]
    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    public NetworkVariable<int> _currentHintIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //private NetworkVariable<bool> _stopHintCoroutine = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Sounds")]
    [SerializeField, Expandable] private SoundSO _SFXValidationLight;

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnKeyboardPlacedOnSocket;
    public UnityEvent OnKeyUsed;
    public UnityEvent OnFailedPassword;
    public UnityEvent OnStateComplete;

    private void Start()
    {
        ClearDisplayPasswordOnComputer();
    }

    public void StartState()
    {
        OnStateStart?.Invoke();
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
    }

    public void AddKey(PASSWORDKEYS key)
    {
        if (_isCoroutineRunning || GameState.Instance.CurrentGameState!= GameState.GAMESTATES.PASSWORD)
            return;


        if (_ignoreKeysAmount > 0)
        {
            _ignoreKeysAmount--;
            return;
        }
        
        _currentPassword.Add(key);
        DisplayKeyOnComputer(key);
        Debug.Log(key);
        OnKeyUsedClientRpc();


        if (_currentPassword.Count == _correctPassword.Count)
        {
            for (int i = 0; i < _correctPassword.Count; i++)
            {
                if (_correctPassword[i] != _currentPassword[i])
                {
                    OnFailedPasswordClientRpc();
                    return;
                }
            }
            //Correct Password
            CorrectPasswordClientRpc();
            OnStateCompleteClientRpc();
            GameState.Instance.ChangeState(GameState.GAMESTATES.LAUNCH);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void CorrectPasswordClientRpc()
    {
        StartCoroutine(ClearPasswordAfterSeconds(_clearPasswordAfter));
        StartCoroutine(FlashingLightCoroutine(true));
        if(IsOwner)
            _currentHintIndex.Value++;
    }

    [ClientRpc]
    private void OnFailedPasswordClientRpc()
    {
        _currentPassword.Clear();
        StartCoroutine(ClearPasswordAfterSeconds(_clearPasswordAfter));
        StartCoroutine(FlashingLightCoroutine(false));
        OnFailedPassword?.Invoke();
    }

    #region EventsRpc
    [ClientRpc]
    private void OnKeyUsedClientRpc()
    {
        OnKeyUsed?.Invoke();
    }

    [Rpc(SendTo.Everyone)]
    public void OnStateCompleteClientRpc()
    {
        OnStateComplete?.Invoke();
    } 
    #endregion

    public void WrongPasswordVisualFeedback(bool success)
    {
        StartCoroutine(FlashingLightCoroutine(success));
    }

    private IEnumerator ClearPasswordAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _currentPassword.Clear();
        ClearDisplayPasswordOnComputer();
    }

    private IEnumerator FlashingLightCoroutine(bool success = false)
    {
        _isCoroutineRunning = true;
        Color targetColor;
        float t;
        
        switch (success)
        {
            case true:
                targetColor = Color.green;
                break;
            case false:
                targetColor = Color.red;
                break;
        }

        for (float i = 0; i < 255; i++)
        {
            t = (i / 255) * _lightFlashingSpeed * Time.deltaTime;
            if (t > 0.95f)
                break;
            
            _light.color = Color.Lerp(_light.color, targetColor, t);
            yield return null;
        }

        t = 0;

        for (float i = 0; i < 255; i++)
        {
            t = (i / 255) * _lightFlashingSpeed * Time.deltaTime;
            if (t > 0.95f)
                break;
            
            _light.color = Color.Lerp(_light.color, Color.white, t);
            yield return null;
        }

        _isCoroutineRunning = false;
    }

    public void CallKeyboardActivated(GameObject anchor)
    {
        anchor.SetActive(false);
        //anchor.GetComponent<XRSocketInteractor>().enabled = false;
        _keyboardToActivate.SetActive(true);
        _keyboardToDeactivate.GetComponentInChildren<MeshRenderer>().enabled = false;
        OnKeyboardPlacedOnSocket?.Invoke();
    }

    public void DisplayKeyOnComputer(PASSWORDKEYS key)
    {
        PasswordSprites passSprite = GetPasswordSpriteFromKey(key);
        _computerSpritesParent.GetChild(_currentPassword.Count - 1).GetComponent<Image>().sprite = passSprite.Sprite;
    }

    private PasswordSprites GetPasswordSpriteFromKey(PASSWORDKEYS key)
    {
        for (int i = 0; i < passwordsSprites.Count; i++)
        {
            if (passwordsSprites[i].Key == key)
                return passwordsSprites[i];
        }

        return passwordsSprites[0];
    }

    public void ClearDisplayPasswordOnComputer()
    {
        foreach (var child in _computerSpritesParent.GetComponentsInChildren<Image>(true))
        {
            child.sprite = _spriteBarre;
        }
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
                    if (_currentHintIndex.Value > _voicesHint.Count - 1)
                    {
                        yield break;
                    }
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
    //[ServerRpc(RequireOwnership = false)]
    //public void ChangeStopHintCoroutineServerRpc(bool value)
    //{
    //    _stopHintCoroutine.Value = value;
    //}
}
