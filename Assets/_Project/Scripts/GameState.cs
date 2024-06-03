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

public class GameState : NetworkBehaviour
{
    public static GameState instance;

    [Header("Variables")]
    [SerializeField] int _launchButtonTimingTolerance;

    [Header("References")]
    [SerializeField] List<GameObject> _launchButtons = new List<GameObject>();
    [SerializeField] VideoPlayer _videoObject;

    VideoPlayer _video;
    ulong _firstClientToPushButtonID = 150;
    DateTime _firstButtonPressTime;
    Coroutine _toleranceCoroutine = null;

    [SerializeField] TextMeshProUGUI _notifText;

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
                break;
            case GAMESTATES.SEPARATION:
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
    }

    private void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //_video = _videoObject.GetComponent<VideoPlayer>();
        //_video.Stop();
        //_videoObject.SetActive(false);
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


    private void PlayVideo()
    {
        if (CurrentGameState == GAMESTATES.LAUNCH)
        {
            _videoObject.gameObject.SetActive(true);
            _videoObject.Play();
        }
    }

    public void ApplyStateChanges(GAMESTATES state)
    {
        CurrentGameState = state;
        Debug.Log(CurrentGameState);
        
        switch (state)
            {
                case GAMESTATES.PASSWORD:
                    break;
                case GAMESTATES.CALIBRATE:
                    //Activate all elements related to calibrating
                    break;
                case GAMESTATES.LAUNCH:
                    FindObjectOfType<Launch>().CanAttach = true;
                    foreach (GameObject item in _launchButtons)
                    {
                        item.GetComponent<VideoPlayerButton>().CanLaunch = true;
                    }
                break;
                case GAMESTATES.VALVES:
                    
                    break;
            case GAMESTATES.SIMONSAYS:
                    FindObjectOfType<Simon>().CanChooseColor = true;
                    FindObjectOfType<Simon>().StartSimonClientRpc();
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
        
        Debug.LogError(CurrentGameState);
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

    [Rpc(SendTo.Everyone)]
    public void CheckLaunchButtonTimingRpc(DateTime givenTime, ulong clientID)
    {
        
        if (DateTime.Equals(_firstButtonPressTime, DateTime.MinValue))
        {
            if (_toleranceCoroutine == null)
                _toleranceCoroutine = StartCoroutine(ButtonPushedCoroutine());

            _firstClientToPushButtonID = clientID;
            _firstButtonPressTime = givenTime;
        }
        else
        {
            if (clientID == _firstClientToPushButtonID)
            {
                Debug.LogError("Same Client called twice");
                _firstClientToPushButtonID = 150;
                _firstButtonPressTime = DateTime.MinValue;
                StopCoroutine(_toleranceCoroutine);
                _toleranceCoroutine = null;
            }
            else
            {
                Debug.LogError("First time: " + _firstButtonPressTime+ "\nSecond time: " + givenTime);
                if (givenTime.Subtract(_firstButtonPressTime).TotalMilliseconds <= _launchButtonTimingTolerance)
                {
                    PlayVideo();
                    ChangeState(GAMESTATES.SIMONSAYS);
                }
                
                _firstClientToPushButtonID = 150;
                _firstButtonPressTime = DateTime.MinValue;
                StopCoroutine(_toleranceCoroutine);
                _toleranceCoroutine = null;
            }

        }
    }

    private IEnumerator ButtonPushedCoroutine()
    {
        yield return new WaitForSeconds(_launchButtonTimingTolerance / 1000);
        _firstClientToPushButtonID = 150;
        _firstButtonPressTime = DateTime.MinValue;
        yield return null;
    }
}
