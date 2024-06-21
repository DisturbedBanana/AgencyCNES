using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.Services.Lobbies.Models;
using NaughtyAttributes;
using Unity.Services.Lobbies;

public class PlayersLobby : NetworkBehaviour
{
    [SerializeField] private NetworkConnect _networkConnect;

    [Header("Player Spawn")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private bool enableSpawnPosition;
    [SerializeField] private List<PlayerSpawn> spawns;
    [SerializeField] private float _waitTimeEndGameTP;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _readyToStart;
    [SerializeField] private Button _buttonToStartGame;
    [SerializeField] private TextMeshProUGUI _notif;
    [SerializeField] private GameObject _notifPanel;
    [SerializeField] private GameObject _canvasWin;

    [Header("Buttons Parent")]
    [SerializeField] private GameObject _layoutParent;
    [SerializeField] private GameObject _returnButtonParent;
    [SerializeField] private GameObject _createButtonParent;
    [SerializeField] private GameObject _joinButtonParent;
    [SerializeField] private GameObject _refreshLobbies;
    [SerializeField] private GameObject _readyToStartParent;
    [SerializeField] private GameObject _QuitGameParent;

    [Header("Button")]
    [SerializeField] private GameObject _lobbyButtonPrefab;

    private List<GameObject> _lobbyButtons;

    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += PlayerConnected;
        NetworkManager.OnClientDisconnectCallback += PlayerDisconnected;
        NetworkConnect.OnCreateLobbySuccess += OnCreateLobbySuccess;
        NetworkConnect.OnJoinLobbySuccess += OnJoinLobbySuccess;

    }

    private void OnDisable()
    {
        NetworkConnect.OnCreateLobbySuccess -= OnCreateLobbySuccess;
        NetworkConnect.OnJoinLobbySuccess -= OnJoinLobbySuccess;
    }

    public void CreateLobby()
    {
        _networkConnect.Create();
        _createButtonParent.GetComponentInChildren<Button>().interactable = false;
        _joinButtonParent.GetComponentInChildren<Button>().interactable = false;
    }
    public void JoinLobby()
    {
        _returnButtonParent.SetActive(true);
        _joinButtonParent.SetActive(false);
        _createButtonParent.SetActive(false);
        RefreshLobbies();
    }

    public async void RefreshLobbies()
    {
        RemoveAllLobbiesButtons();

        _lobbyButtons = new List<GameObject>();
        var lobbies = await _networkConnect.SearchAllLobbiesAvailable();

        foreach (var lobby in lobbies.Results.OrderBy(lobbyOrder => lobbyOrder.Created))
        {
            if (lobby.AvailableSlots == 0)
                continue;
            Debug.Log($"Lobby: {lobby.Data["JOIN_CODE"].Value} - {lobby.Players.Count}/{lobby.MaxPlayers} - {lobby.Name} - {lobby.MaxPlayers}");
            GameObject lobbyButton = Instantiate(_lobbyButtonPrefab, _layoutParent.transform);
            lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Join lobby:{lobby.Data["JOIN_CODE"].Value} - {lobby.Players.Count}/{lobby.MaxPlayers}";
            lobbyButton.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonJoinALobby(lobby.Data["JOIN_CODE"].Value));
            _lobbyButtons.Add(lobbyButton);
        }
    }

    private void ButtonJoinALobby(string lobbyCode)
    {
        _networkConnect.JoinWithCode(lobbyCode);

        foreach (var button in _lobbyButtons)
        {
            button.GetComponentInChildren<Button>().interactable = false;
        }
    }

    private void RemoveAllLobbiesButtons()
    {
        if (_lobbyButtons != null)
        {
            foreach (var button in _lobbyButtons)
            {
                Destroy(button);
            }
        }
    }

    public void Return()
    {
        RemoveAllLobbiesButtons();
        _returnButtonParent.SetActive(false);
        _joinButtonParent.SetActive(true);
        _createButtonParent.SetActive(true);

    }

    private void OnCreateLobbySuccess(bool success)
    {
        if (success)
        {
            _createButtonParent.SetActive(false);
            _joinButtonParent.SetActive(false);
            _refreshLobbies.SetActive(false);
            _readyToStartParent.SetActive(true);
        }
        else
        {
            _notif.text = "Failed to create lobby";
            _createButtonParent.GetComponentInChildren<Button>().interactable = true;
            _joinButtonParent.GetComponentInChildren<Button>().interactable = true;
            Debug.Assert(true == _createButtonParent.GetComponentInChildren<Button>(true).interactable);
            Debug.Assert(true == _joinButtonParent.GetComponentInChildren<Button>(true).interactable);
        }
    }

    private void OnJoinLobbySuccess(bool success)
    {
        if (success)
        {
            _createButtonParent.SetActive(false);
            _joinButtonParent.SetActive(false);
            _refreshLobbies.SetActive(false);
            _returnButtonParent.SetActive(false);
            _readyToStartParent.SetActive(true);
            foreach (var button in _lobbyButtons)
            {
                Destroy(button);
            }
        }
        else
        {
            _notif.text = "Failed to join lobby";
            foreach (var button in _lobbyButtons)
            {
                button.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void DisplayTextNotif(string text)
    {
        _notif.text += $"{text}\n";
        SyncNotifMessagesRpc(_notif.text);
    }
    private void PlayerConnected(ulong id)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        CheckStartAGame();
        //DisplayTextNotif("Player connected: " + NetworkManager.Singleton.ConnectedClients.First(client => client.Value.ClientId == id));
    }
    private void PlayerDisconnected(ulong id)
    {
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
            if (NetworkManager.Singleton.IsHost)
                _buttonToStartGame.interactable = true;
        }
        if(NetworkManager.Singleton.IsHost)
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
        if (enableSpawnPosition)
        {
            PlayerSpawn playerSpawn = ChooseAMovementTypeToSpawnPlayer();
            _playerController.SpawnPlayer(playerSpawn);
        }

        GameState.Instance.StartWithState();
    }

    public void TeleportPlayerDebug(int num)
    {

        PlayerSpawn playerSpawn = spawns[num];
        _playerController.SpawnPlayer(playerSpawn);
    }

    private PlayerSpawn ChooseAMovementTypeToSpawnPlayer()
    {
        bool isHost = NetworkManager.Singleton.IsHost;
        PlayerSpawn playerSpawn = isHost ? spawns.First(x => x.MovementType == PlayerController.MOVEMENTTYPE.LAUNCHER)
            : spawns.First(x => x.MovementType == PlayerController.MOVEMENTTYPE.MISSIONCONTROL);
        return playerSpawn;
    }

    [Rpc(SendTo.Everyone)]
    public void EndGameRpc()
    {
        StartCoroutine(WaitBeforeTPToLobby());
    }


    private IEnumerator WaitBeforeTPToLobby()
    {
        yield return new WaitForSeconds(_waitTimeEndGameTP);
        var tpsLobby = spawns.Where(x => x.MovementType == PlayerController.MOVEMENTTYPE.LOBBY);
        PlayerSpawn playerSpawn = NetworkManager.Singleton.LocalClientId == 0 ? tpsLobby.First() : tpsLobby.Last();
        _playerController.SpawnPlayer(playerSpawn);

        _canvasWin.SetActive(true);
        _QuitGameParent.SetActive(true);
        _returnButtonParent.SetActive(false);
        _createButtonParent.SetActive(false);
        _joinButtonParent.SetActive(false);
        _refreshLobbies.SetActive(false);
        _readyToStartParent.SetActive(false);
        _notifPanel.SetActive(false);
    }

    public async void QuitGame()
    {
        //quit application

        if (NetworkManager.Singleton.IsHost) 
            await LobbyService.Instance.DeleteLobbyAsync(NetworkManager.Singleton.GetComponent<NetworkConnect>().CurrentLobby.Id);

        Application.Quit();
    }
}
