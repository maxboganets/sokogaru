using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SokogaruNetworkManager : NetworkManager
{
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
