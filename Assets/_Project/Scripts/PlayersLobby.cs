using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayersLobby : NetworkBehaviour
{
    [SerializeField] private NetworkConnect _networkConnect;

    [Header("Player Spawn")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private bool enableSpawnPosition;
    [SerializeField] private List<PlayerSpawn> spawns;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _readyToStart;
    [SerializeField] private Button _buttonToStartGame;
    [SerializeField] private TextMeshProUGUI _notif;

    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += PlayerConnected;
        NetworkManager.OnClientDisconnectCallback += PlayerDisconnected;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.OnClientConnectedCallback -= PlayerConnected;
        NetworkManager.OnClientDisconnectCallback -= PlayerDisconnected;
    }

    private void DisplayTextNotif(string text)
    {
        _notif.text += $"{text}\n";
        SyncNotifMessagesRpc(_notif.text);
    }
    private void PlayerConnected(ulong id)
    {
        if(NetworkManager.Singleton.IsHost)
            CheckStartAGame();
    }
    private void PlayerDisconnected(ulong id)
    {
        if (NetworkManager.Singleton.IsHost)
            CheckStartAGame();
    }

    private void CheckStartAGame()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < 2)
        {
            _readyToStart.text = $"Awaiting players to start {NetworkManager.Singleton.ConnectedClients.Count}/2";
            _buttonToStartGame.interactable = false;
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            _readyToStart.text = $"Ready to start {NetworkManager.Singleton.ConnectedClients.Count}/2";
            _buttonToStartGame.interactable = true;
        }
        SyncButtonGameRpc(_readyToStart.text);
    }

    [Rpc(SendTo.NotServer)]
    private void SyncNotifMessagesRpc(string message)
    {
        _notif.text = message;
    }

    [Rpc(SendTo.NotServer)]
    private void SyncButtonGameRpc(string message)
    {
        _readyToStart.text = message;
    }

    public void StartAGame()
    {
        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        Debug.LogError("StartGameClientRpc");
        if (!enableSpawnPosition)
            return;

        PlayerSpawn playerSpawn = ChooseAMovementTypeToSpawnPlayer();
        _playerController.SpawnPlayer(playerSpawn);
        GameState.Instance.StartWithState();
    }

    private PlayerSpawn ChooseAMovementTypeToSpawnPlayer()
    {
        bool isHost = NetworkManager.Singleton.IsHost;
        PlayerSpawn playerSpawn = isHost ? spawns.First(x => x.MovementType == PlayerController.MOVEMENTTYPE.LAUNCHER)
            : spawns.First(x => x.MovementType == PlayerController.MOVEMENTTYPE.MISSIONCONTROL);
        return playerSpawn;
    }
}
