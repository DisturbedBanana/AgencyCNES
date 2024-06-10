using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class PasswordPuzzleManager : MonoBehaviour, IGameState
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
        OnKeyUsed?.Invoke();


        if (_currentPassword.Count == _correctPassword.Count)
        {
            for (int i = 0; i < _correctPassword.Count; i++)
            {
                if (_correctPassword[i] != _currentPassword[i])
                {
                    //Wrong password
                    _currentPassword.Clear();
                    StartCoroutine(ClearPasswordAfterSeconds(_clearPasswordAfter));
                    StartCoroutine(FlashingLightCoroutine(false));
                    OnFailedPassword?.Invoke();
                    return;
                }
            }
            //Correct Password
            GameState.Instance.ChangeState(GameState.GAMESTATES.LAUNCH);
            StartCoroutine(ClearPasswordAfterSeconds(_clearPasswordAfter));
            StartCoroutine(FlashingLightCoroutine(true));
            OnStateComplete?.Invoke();
        }
    }

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

    public void StartState()
    {
        OnStateStart?.Invoke();
    }
}
