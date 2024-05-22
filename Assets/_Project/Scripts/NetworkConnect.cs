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

public class NetworkConnect : MonoBehaviour
{
    public int maxConnections = 20;
    public UnityTransport transport;

    public GameObject prefabObjects;

    private Lobby _currentLobby = null;

    private float _heartBeatTimer;

    [SerializeField] private TextMeshProUGUI m_TextMeshProUGUI;


    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (NetworkManager.Singleton == null)
            return;
        NetworkManager.Singleton.OnServerStarted += () => DisplayText("Server Started");
        NetworkManager.Singleton.OnServerStopped += (bool isstopped) => DisplayText("Server Stopped");
        //NetworkManager.Singleton.OnClientStarted += () => DisplayText("Client Started");
        //NetworkManager.Singleton.OnClientStopped += (bool isstopped) => DisplayText("Client Stopped");
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong id) => DisplayText("A player connected");
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong id) => DisplayText("A player disconnected");

        //JoinOrCreate();
    }

    public void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;
        NetworkManager.Singleton.OnServerStarted -= () => DisplayText("Server Started");
        NetworkManager.Singleton.OnServerStopped -= (bool isstopped) => DisplayText("Server Stopped");
        //NetworkManager.Singleton.OnClientStarted -= () => DisplayText("Client Started");
        //NetworkManager.Singleton.OnClientStopped -= (bool isstopped) => DisplayText("Client Stopped");
        NetworkManager.Singleton.OnClientConnectedCallback -= (ulong id) => DisplayText("A player connected");
        NetworkManager.Singleton.OnClientDisconnectCallback -= (ulong id) => DisplayText("A player disconnected");
    }

    private void DisplayText(string text)
    {
        m_TextMeshProUGUI.text += $"{text}\n";
    }

    
    public async void JoinOrCreate()
    {
        try
        {
            _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = _currentLobby.Data["JOIN_CODE"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch
        {
            Create();
        }
    }

    public async void Create()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.LogError("JoinCode: " + newJoinCode);
        transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort) allocation.RelayServer.Port,allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
        lobbyOptions.IsPrivate = false;
        lobbyOptions.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
        lobbyOptions.Data.Add("JOIN_CODE", dataObject);

        _currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnections, lobbyOptions);
        NetworkManager.Singleton.StartHost();
    }

    public async void Join()
    {
        try
        {
            _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = _currentLobby.Data["JOIN_CODE"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogException(e);

        }

    }


    private void Update()
    {
        if(_heartBeatTimer > 15)
        {
            _heartBeatTimer -= 15;

            if (_currentLobby != null && _currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }

        _heartBeatTimer += Time.deltaTime;
    }
}
