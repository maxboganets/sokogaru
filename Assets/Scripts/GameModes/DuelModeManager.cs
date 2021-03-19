using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Sokogaru.Lobby
{
    public class DuelModeManager : GameManager
    {
        public override void StartMatch()
        {
            Debug.Log("<color=green>StartGame()</color>");
            this.SpawnPlayers();
        }

        public void SpawnPlayers()
        {
            this.ClientSpawnPlayers();
        }

        void ClientSpawnPlayers()
        {
            Debug.Log($"<color=red>>>>>>>>> PLAYERS: </color>" + this.players.Count);
            foreach (var player in this.players)
            {
                var prefab = UILobby.instance.charactersPrefabs[player.characterIndex];
                Vector3 playerSpawnPosition = GameObject.Find($"Player{player.playerIndex.ToString()}Spawn").transform.position;
                GameObject playerCharacterObject = Instantiate(prefab, playerSpawnPosition, Quaternion.identity);
                NetworkIdentity m_Identity = player.GetComponent<NetworkIdentity>();
                NetworkServer.Spawn(playerCharacterObject, m_Identity.connectionToClient);
                playerCharacterObject.GetComponent<NetworkIdentity>().AssignClientAuthority(m_Identity.connectionToClient);
            }
        }
    }
}