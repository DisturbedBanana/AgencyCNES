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

public class NetworkConnect : MonoBehaviour
{
    public int maxConnections = 20;
    public UnityTransport transport;

    public TextMeshProUGUI m_TextMeshProUGUI;
    public GameObject prefabObjects;

    private Lobby _currentLobby;

    private float _heartBeatTimer;

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinOrCreate();
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
            m_TextMeshProUGUI.text += "StartClient NetworkConnect\n";
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
        m_TextMeshProUGUI.text += "StartHost NetworkConnect\n";
    }

    public async void Join()
    {
        _currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        string relayJoinCode = _currentLobby.Data["JOIN_CODE"].Value;

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
        m_TextMeshProUGUI.text += "StartClient NetworkConnect\n";
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
