using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Sokogaru.Lobby
{
    abstract public class GameManager : NetworkBehaviour
    {
        public SyncList<Player> players = new SyncList<Player>();

        public void AddPlayer(Player _player)
        {
            this.players.Add(_player);
        }

        public abstract void StartMatch();
    }

    //public class GameManager : NetworkBehaviour
    //{
    //    public SyncList<Player> players = new SyncList<Player>();

    //    public void AddPlayer(Player _player)
    //    {
    //        this.players.Add(_player);
    //    }

    //    public void SpawnPlayers()
    //    {
    //        this.ClientSpawnPlayers();
    //    }

    //    //[ClientRpc]
    //    void ClientSpawnPlayers()
    //    {
    //        Debug.Log($"<color=red>>>>>>>>> PLAYERS: </color>" + this.players.Count);
    //        foreach (var player in this.players)
    //        {
    //            var prefab = UILobby.instance.charactersPrefabs[player.characterIndex];
    //            Vector3 playerSpawnPosition = GameObject.Find($"Player{player.playerIndex.ToString()}Spawn").transform.position;
    //            GameObject playerCharacterObject = Instantiate(prefab, playerSpawnPosition, Quaternion.identity);
    //            NetworkIdentity m_Identity = player.GetComponent<NetworkIdentity>();
    //            NetworkServer.Spawn(playerCharacterObject, m_Identity.connectionToClient);
    //            playerCharacterObject.GetComponent<NetworkIdentity>().AssignClientAuthority(m_Identity.connectionToClient);
    //        }
    //    }
    //}
}