using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using UnityEngine.Serialization;
using Unity.XR.CoreUtils;
using NaughtyAttributes;
using System.Linq;
using UnityEngine.Rendering;
using System.Threading.Tasks;

public class NetworkConnect : MonoBehaviour
{
    public int maxConnections = 20;
    [SerializeField] private UnityTransport _unityTransport;

    private Lobby _currentLobby;

    private float _heartBeatTimer;

    [SerializeField] private TextMeshProUGUI m_TextMeshProUGUI;
    [SerializeField] private string _LobbyCode;

    //[Header("UEvents")]
    public static event Action<bool> OnCreateLobbySuccess;
    public static event Action<bool> OnJoinLobbySuccess;


    private async void Awake()
    {
        _currentLobby = null;
        await UnityServices.InitializeAsync();
        string playerName = $"Player_{UnityEngine.Random.Range(0, 100)}";
        AuthenticationService.Instance.SwitchProfile(playerName);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (NetworkManager.Singleton == null)
            return;
        NetworkManager.Singleton.OnServerStarted += () => DisplayText("Server Started");
        NetworkManager.Singleton.OnServerStopped += (bool isstopped) => DisplayText("Server Stopped");
        //NetworkManager.Singleton.OnClientStarted += () => DisplayText("Client Started");
        //NetworkManager.Singleton.OnClientStopped += (bool isstopped) => DisplayText("Client Stopped");
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong id) => DisplayText("A player disconnected");

        //JoinOrCreate();

        _heartBeatTimer = 0f;
    }

    public void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;
        NetworkManager.Singleton.OnServerStarted -= () => DisplayText("Server Started");
        NetworkManager.Singleton.OnServerStopped -= (bool isstopped) => DisplayText("Server Stopped");
        //NetworkManager.Singleton.OnClientStarted -= () => DisplayText("Client Started");
        //NetworkManager.Singleton.OnClientStopped -= (bool isstopped) => DisplayText("Client Stopped");
        NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= (ulong id) => DisplayText("A player disconnected");
    }

    private void DisplayText(string text)
    {
        m_TextMeshProUGUI.text += $"{text}\n";
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendInfoToServerRpc(string text)
    {
        Debug.Log("Server info: " + text);
    }

    private void PlayerConnected(ulong id)
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.LocalClientId == id)
            return;
        if(!NetworkManager.Singleton.IsHost)
            DisplayText("Server joined");
        SendInfoToServerRpc($"Client {NetworkManager.Singleton.LocalClient.ClientId} connected");
    }


    public async void JoinOrCreate()
    {
        try
        {
            _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = _currentLobby.Data["JOIN_CODE"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            _unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch
        {
            Create();
        }
    }

    [Button]
    public async void Create()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            _unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
            
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>();
            DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
            lobbyOptions.Data.Add("JOIN_CODE", dataObject);

            _currentLobby = await Lobbies.Instance.CreateLobbyAsync("Kourou lobby", maxConnections, lobbyOptions);
            if (NetworkManager.Singleton.StartHost())
                OnCreateLobbySuccess?.Invoke(true);
            
            DisplayText($"Lobby created with code {newJoinCode}");
            //Debug.LogError($"Lobby created with code {newJoinCode}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            OnCreateLobbySuccess?.Invoke(false);

        }
    }

    [Button]
    public async void Join()
    {
        try
        {
            _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = _currentLobby.Data["JOIN_CODE"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            _unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            if(NetworkManager.Singleton.StartClient())
                OnJoinLobbySuccess?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            DisplayText("Error trying to connect. Please retry.");
            OnJoinLobbySuccess?.Invoke(false);

        }

    }
    [Button]
    public async void JoinWithCode()
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(_LobbyCode);
            _unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            if (NetworkManager.Singleton.StartClient())
                OnJoinLobbySuccess?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            OnJoinLobbySuccess?.Invoke(false);
        }
    }
    public async void JoinWithCode(string loobyCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(loobyCode);
            _unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            if (NetworkManager.Singleton.StartClient())
                OnJoinLobbySuccess?.Invoke(true);


        }
        catch (Exception e)
        {
            Debug.LogException(e);
            OnJoinLobbySuccess?.Invoke(false);

        }
    }

    [Button]
    public async Task<QueryResponse> SearchAllLobbiesAvailable()
    {
        var lobbies = await Lobbies.Instance.QueryLobbiesAsync();

        foreach (var lobby in lobbies.Results)
        {
            Debug.Log($"Lobby: {lobby.LobbyCode} - {lobby.Name} - {lobby.Data["JOIN_CODE"].Value}");
        }
        return lobbies;
    }

    private void Update()
    {
        if(_heartBeatTimer > 15)
        {
            _heartBeatTimer -= 15;
            if(NetworkManager.Singleton.IsHost)
                LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }

        _heartBeatTimer += Time.deltaTime;
    }
}
