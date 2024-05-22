using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem.LowLevel;

public class GameState : NetworkBehaviour
{
    public static GameState instance;

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
    }

    #region PROPERTIES

    GAMESTATES _currentGameState = GAMESTATES.PASSWORD;
    public GAMESTATES CurrentGameState
    {
        get { return _currentGameState; }
        set { _currentGameState = value; }
    }
    #endregion

    [SerializeField] List<GameObject> LaunchButtons = new List<GameObject>();

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

    public void ApplyStateDebug(int debug)
    {
        ApplyStateChanges(GAMESTATES.PASSWORD, true, debug);
    }

    public void ApplyStateChanges(GAMESTATES state = GAMESTATES.PASSWORD, bool isDebug = false, int debugID = 0)
    {
        if (isDebug)
        {
            switch (debugID)
            {
                case 1:
                    CurrentGameState = GAMESTATES.LAUNCH;
                    break;
                case 2:
                    CurrentGameState = GAMESTATES.VALVES;
                    break;
                case 3:
                    CurrentGameState = GAMESTATES.SIMONSAYS;
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (state)
            {
                case GAMESTATES.PASSWORD:
                    break;
                case GAMESTATES.CALIBRATE:
                    CurrentGameState = GAMESTATES.CALIBRATE;
                    //Activate all elements related to calibrating
                    break;
                case GAMESTATES.LAUNCH:
                    //Change control video (launch video)
                    //When harness is attached and button pressed -> valves (coroutine for timer?)
                    break;
                case GAMESTATES.VALVES:
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
        Debug.LogError(CurrentGameState);
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
