using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Sokogaru.Networking
{
    public class NetworkRoomPlayer : NetworkBehaviour
    {

    }

    public class NetworkGamePlayer : NetworkBehaviour
    {

    }

    public class SokogaruNetworkManager : NetworkManager
    {
        [SerializeField] private int minPlayers = 2;
        [Scene] [SerializeField] private string menuScene = string.Empty;

        [Header("Room")]
        [SerializeField] private NetworkRoomPlayer networkRoomPlayer;

        [Header("Game")]
        [SerializeField] private NetworkGamePlayer networkGamePlayer;

        public override void OnStartServer()
        {
            Debug.Log("Server Started!");
        }

        public override void OnStopServer()
        {
            Debug.Log("Server Stopped!");
        }

        public override void OnClientConnect(NetworkConnection connection)
        {
            Debug.Log("Client Started!");
        }

        public override void OnClientDisconnect(NetworkConnection connection)
        {
            Debug.Log("Client Stopped!");
        }
    }
}
