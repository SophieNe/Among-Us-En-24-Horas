using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
public class Lobby : NetworkBehaviour
{
    public NetworkIdentity playerPrefab;
    public PlayerLobby[] spawnPoints;
    bool[] isSpawnFull = new bool[10];
    Dictionary<int, string> serverPlayerID = new Dictionary<int, string>();
    public Text playersCount, lobbyCode;
    public Button ready, startGame;
    PlayerLobby myPlayerLobby;
    NetworkRoomPlayerExt myPlayerNetwork;
    NetworkRoomManagerExt roomManager;
    UnityAction startGameUpdate;
    public void SetLobby(bool isHost, NetworkRoomPlayerExt myNetwork)
    {
        roomManager = NetworkRoomManagerExt.singleton as NetworkRoomManagerExt;
        myPlayerNetwork = myNetwork;
        myPlayerLobby = GetMyPlayerLobby();

        playersCount.text = GetPlayerCount().ToString();

        SetLobbyCode(roomManager.networkAddress);

        ready.onClick.AddListener(GetReady);

        if (isHost)
        {
            startGame.gameObject.SetActive(true);
            startGame.onClick.AddListener(delegate { roomManager.ServerChangeScene(roomManager.GameplayScene); });
            startGameUpdate = isStartInteractable;
        }
        else
        {
            startGame.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (startGameUpdate != null)
        {
            startGameUpdate.Invoke();
        }
    }
    void GetReady()
    {
        myPlayerNetwork.CmdChangeReadyState(true);
        myPlayerLobby.SetReady(true);
        ready.gameObject.SetActive(false);
    }
    PlayerLobby GetMyPlayerLobby()
    {
        foreach (PlayerLobby player in spawnPoints)
        {
            Debug.Log(myPlayerNetwork.playerName + " and " + player.playerName);
            if (player.playerName.Equals(myPlayerNetwork.playerName))
            {
                return player;
            }
            else if (string.IsNullOrEmpty(player.playerName) || player.playerName.Equals(player.DefaultName()))
            {
                player.SetName(myPlayerNetwork.playerName);
                return player;
            }
        }
        return null;
    }
    void isStartInteractable()
    {
        startGame.interactable = roomManager.allPlayersReady;
    }
    [Command(ignoreAuthority = true)]
    public void ServerAddPlayerToLobby(string playerName, int connectionID)
    {
        int emptySlot = FindNextEmptySlot();

        serverPlayerID.Add(connectionID, playerName);

        spawnPoints[emptySlot].SetName(playerName);
        spawnPoints[emptySlot].SetReady(false);

        isSpawnFull[emptySlot] = true;

        RpcChangePlayersCount(GetPlayerCount().ToString());
    }
    [Command(ignoreAuthority = true)]
    public void ServerDeletePlayerFromLobby(string playerName, int connectionID)
    {
        int index = GetPlayerFromName(playerName);

        spawnPoints[index].SetName(spawnPoints[index].DefaultName());
        isSpawnFull[index] = false;

        serverPlayerID.Remove(connectionID);

        RpcChangePlayersCount(GetPlayerCount().ToString());
    }
    int FindNextEmptySlot()
    {
        for (int i = 0; i < isSpawnFull.Length; i++)
        {
            if (!isSpawnFull[i])
            {
                return i;
            }
        }
        return 0;
    }
    [ClientRpc]
    void RpcChangePlayersCount(string count)
    {
        playersCount.text = count;
    }
    int GetPlayerCount()
    {
        return NetworkServer.connections.Count;
    }
    int GetPlayerFromName(string playName)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].playerName.Equals(playName))
            {
                return i;
            }
        }
        return 0;
    }
    public string GetPlayerNameFromConn(int connID)
    {
        return serverPlayerID[connID];
    }
    public void SetLobbyCode(string code)
    {
        lobbyCode.text = "Codigo de la sala: " + code;
    }
}
