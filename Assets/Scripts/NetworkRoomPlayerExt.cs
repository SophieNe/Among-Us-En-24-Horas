using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Game;

namespace Mirror
{
    [AddComponentMenu("")]
    public class NetworkRoomPlayerExt : NetworkRoomPlayer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkRoomPlayerExt));
        [HideInInspector] public Lobby lobby;
        public override void OnStartClient()
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "OnStartClient {0}", SceneManager.GetActiveScene().path);

            base.OnStartClient();
        }
        public override void OnClientEnterRoom()
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "OnClientEnterRoom {0}", SceneManager.GetActiveScene().path);
            string playName = Game.PlayerPrefs.playerName;
            ChangePlayerName(playName);
        }
        public override void OnStartLocalPlayer()
        {
            StartCoroutine(StartLocalDelay());
        }
        IEnumerator StartLocalDelay()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            bool isHosted = NetworkServer.connections.Count == 1;
            yield return new WaitForSecondsRealtime(1f);
            CheckLobbyState();
            Debug.Log(playerName);
            AddPlayerToLobby(playerName);
            lobby.SetLobby(isHosted, this);
        }
        [Command(ignoreAuthority = true)]
        void AddPlayerToLobby(string playName)
        {
            CheckLobbyState();
            lobby.ServerAddPlayerToLobby(playName, connectionToClient.connectionId);
        }
        public override void OnClientExitRoom()
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "OnClientExitRoom {0}", SceneManager.GetActiveScene().path);

            CheckLobbyState();
            Debug.Log(playerName);
        }

        public override void ReadyStateChanged(bool _, bool newReadyState)
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "ReadyStateChanged {0}", newReadyState);
            
        }
        public void CheckLobbyState()
        {
            if (lobby == null)
            {
                lobby = FindObjectOfType<Lobby>();
            }
        }
    }
}
