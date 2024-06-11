using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem.LowLevel;
using System;
using UnityEngine.Video;
using TMPro;
using NaughtyAttributes;
using System.Security.Cryptography;
using UnityEngine.Events;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    [Header("Variables")]
    [SerializeField] int _launchButtonTimingTolerance;


    [SerializeField] TextMeshProUGUI _notifText;
    [SerializeField] private GAMESTATES _StartWithState;


    [Header("Light")]
    [SerializeField] Light _light;
    [SerializeField, Range(2f, 50f)] float _lightFlashingSpeed = 10f;

    [Header("Events")]
    public UnityEvent OnStateChange;

    public enum GAMESTATES
    {
        PASSWORD, //Control player enters password
        CALIBRATE, //Both player calibrate
        LAUNCH, //Harness and button
        VALVES, //Ship player moves valves to match control gauges
        SIMONSAYS, //Control player activates color in order -> told by ship player
        SEPARATION, //Ship player connects cables according to control player's instructions -> players pull lever together
        FUSES, //Control player activates fuses according to ship player's  instructions
        FREQUENCY, //Both players tune frequency to match other's instructions (easier for ship player)
        DODGE, //Control player controls ship to dodge asteroids, but is guided by ship player (30s)
        WHACKAMOLE, //whack-a-mole game
    }

    #region PROPERTIES

    GAMESTATES _currentGameState = GAMESTATES.PASSWORD;
    private bool _isCoroutineRunning;

    public GAMESTATES CurrentGameState
    {
        get { return _currentGameState; }
        set { _currentGameState = value; }
    }
    #endregion

    public void StartWithState()
    {
        GoToState(_StartWithState);
    }
    public void StateForce(GAMESTATES state)
    {
        CurrentGameState = state;
    }

    [Button]
    public void NextStateForce()
    {
        switch (CurrentGameState)
        {
            case GAMESTATES.PASSWORD:
                ApplyStateChangesRpc(GAMESTATES.LAUNCH);
                break;
            case GAMESTATES.CALIBRATE:
                break;
            case GAMESTATES.LAUNCH:
                ApplyStateChangesRpc(GAMESTATES.VALVES);
                break;
            case GAMESTATES.VALVES:
                ApplyStateChangesRpc(GAMESTATES.SIMONSAYS);
                break;
            case GAMESTATES.SIMONSAYS:
                ApplyStateChangesRpc(GAMESTATES.SIMONSAYS);
                break;
            case GAMESTATES.SEPARATION:
                ApplyStateChangesRpc(GAMESTATES.SEPARATION);
                break;
            case GAMESTATES.FUSES:
                ApplyStateChangesRpc(GAMESTATES.FUSES);
                break;
            case GAMESTATES.FREQUENCY:
                ApplyStateChangesRpc(GAMESTATES.FREQUENCY);
                break;
            case GAMESTATES.DODGE:
                break;
            default:
                break;
        }
    }
    public void GoToState(GAMESTATES state)
    {
        ApplyStateChangesRpc(state);
    }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GAMESTATES state)
    {
        if (IsOwner)
        {
            AskForGameStateUpdateClientRpc(this.GetComponent<NetworkObject>(), state);
        }
        else
        {
            AskForGameStateUpdateServerRpc(this.GetComponent<NetworkObject>(), state);
        }

        ApplyStateChangesRpc(state);
    }

    public void ChangeState(string state)
    {
        foreach (var enumValue in Enum.GetValues(typeof(GAMESTATES)))
        {
            if (enumValue.ToString() == state.ToUpper())
            {
                ChangeState((GAMESTATES)enumValue);
            }
        }
        Debug.LogError("Enum" + state.ToUpper() + " value found in " + typeof(GAMESTATES));
    }


    [Rpc(SendTo.Everyone)]
    public void ApplyStateChangesRpc(GAMESTATES state)
    {
        OnStateChange?.Invoke();
        CurrentGameState = state;

        switch (state)
        {
            case GAMESTATES.PASSWORD:
                FindObjectOfType<PasswordPuzzleManager>().StartState();
                break;
            case GAMESTATES.LAUNCH:
                FindObjectOfType<Launch>().StartState();
                break;
            case GAMESTATES.VALVES:
                FindObjectOfType<ValveManager>().StartState();
                break;
            case GAMESTATES.SIMONSAYS:
                FindObjectOfType<Simon>().StartState();
                break;
            case GAMESTATES.SEPARATION:
                FindObjectOfType<Separation>().StartState();
                break;
            case GAMESTATES.FUSES:
                FindObjectOfType<FuseManager>().StartState();
                break;
            case GAMESTATES.FREQUENCY:
                FindObjectOfType<FrequenciesCheck>().StartState();
                break;
            default:
                Debug.LogError("No State found");
                break;
        }

        Debug.LogError(CurrentGameState);
        _notifText.text += "ChangeState to: " + CurrentGameState;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskForGameStateUpdateServerRpc(NetworkObjectReference networkObjectRef, GAMESTATES state)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().ApplyStateChangesRpc(state);
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = true)]
    private void AskForGameStateUpdateClientRpc(NetworkObjectReference networkObjectRef, GAMESTATES state)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().ApplyStateChangesRpc(state);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void FlashValidationLightRpc(bool success)
    {
        if(_isCoroutineRunning)
            return;

        StartCoroutine(FlashingLightCoroutine(success));
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
            t = (i / 255) * (_lightFlashingSpeed * Time.deltaTime);
            if (t > 0.95f)
                break;

            _light.color = Color.Lerp(_light.color, targetColor, t);
            yield return null;
        }

        t = 0;

        for (float i = 0; i < 255; i++)
        {
            t = (i / 255) * (_lightFlashingSpeed * Time.deltaTime);
            if (t > 0.95f)
                break;

            _light.color = Color.Lerp(_light.color, Color.white, t);
            yield return null;
        }

        _isCoroutineRunning = false;
    }
}
