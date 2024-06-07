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

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    [Header("Variables")]
    [SerializeField] int _launchButtonTimingTolerance;


    [SerializeField] TextMeshProUGUI _notifText;
    [SerializeField] private GAMESTATES _StartWithState;

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
                ApplyStateChanges(GAMESTATES.LAUNCH);
                break;
            case GAMESTATES.CALIBRATE:
                break;
            case GAMESTATES.LAUNCH:
                ApplyStateChanges(GAMESTATES.VALVES);
                break;
            case GAMESTATES.VALVES:
                ApplyStateChanges(GAMESTATES.SIMONSAYS);
                break;
            case GAMESTATES.SIMONSAYS:
                ApplyStateChanges(GAMESTATES.SIMONSAYS);
                break;
            case GAMESTATES.SEPARATION:
                ApplyStateChanges(GAMESTATES.SEPARATION);
                break;
            case GAMESTATES.FUSES:
                ApplyStateChanges(GAMESTATES.FUSES);
                break;
            case GAMESTATES.FREQUENCY:
                ApplyStateChanges(GAMESTATES.FREQUENCY);
                break;
            case GAMESTATES.DODGE:
                break;
            default:
                break;
        }
    }
    public void GoToState(GAMESTATES state)
    {
        ApplyStateChanges(state);
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
            AskForGameStateUpdateClientRpc(this.GetComponent<NetworkObject>(),state);
        }
        else
        {
            AskForGameStateUpdateServerRpc(this.GetComponent<NetworkObject>(),state);
        }

        ApplyStateChanges(state);
    }

    public void ChangeState(string state)
    {
        foreach (var enumValue in Enum.GetValues(typeof(GAMESTATES)))
        {
            if(enumValue.ToString() == state.ToUpper())
            {
                ChangeState((GAMESTATES)enumValue);
            }
        }
        Debug.LogError("Enum"+ state.ToUpper() + " value found in " + typeof(GAMESTATES));
    }


    public void ApplyStateChanges(GAMESTATES state)
    {
        CurrentGameState = state;
        Debug.LogError(CurrentGameState);

        switch (state)
            {
                case GAMESTATES.PASSWORD:
                    break;
                case GAMESTATES.CALIBRATE:
                    //Activate all elements related to calibrating
                    break;
                case GAMESTATES.LAUNCH:
                    FindObjectOfType<Launch>().LaunchCountdownForSitting();
                break;
                case GAMESTATES.VALVES:
                    
                    break;
            case GAMESTATES.SIMONSAYS:
                    FindObjectOfType<Simon>().StartState();
                    break;
                case GAMESTATES.SEPARATION:
                //Change control video (launch video)
                //When harness is attached and button pressed -> valves (coroutine for timer?)
                break;
                case GAMESTATES.WHACKAMOLE:
                    break;
                case GAMESTATES.FUSES:
                    break;
                case GAMESTATES.FREQUENCY:
                    break;
                case GAMESTATES.DODGE:
                    break;
                default:
                    break;
            }
        
        _notifText.text += "ChangeState to: " + CurrentGameState;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskForGameStateUpdateServerRpc(NetworkObjectReference networkObjectRef, GAMESTATES state)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().ApplyStateChanges(state);
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = true)]
    private void AskForGameStateUpdateClientRpc(NetworkObjectReference networkObjectRef, GAMESTATES state)
    {
        if (networkObjectRef.TryGet(out NetworkObject networkObject2))
        {
            networkObject2.GetComponent<GameState>().ApplyStateChanges(state);
        }
    }

}
