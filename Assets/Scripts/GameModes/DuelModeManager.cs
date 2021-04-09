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
            // Spawn Players with delay after scene spawn
            StartCoroutine(this.SpawnPlayers());
        }

        public IEnumerator SpawnPlayers()
        {

            yield return new WaitForSeconds(0.1F);
            foreach (var player in this.players)
            {
                var prefab = UILobby.instance.charactersPrefabs[player.characterIndex];
                Vector3 playerSpawnPosition = GameObject.Find($"Player{player.playerIndex.ToString()}Spawn").transform.position;
                GameObject playerCharacterObject = Instantiate(prefab, playerSpawnPosition, Quaternion.identity);
                NetworkIdentity m_Identity = player.GetComponent<NetworkIdentity>();
                NetworkServer.Spawn(playerCharacterObject, m_Identity.connectionToClient);
                playerCharacterObject.GetComponent<NetworkIdentity>().AssignClientAuthority(m_Identity.connectionToClient);
                this.AssignCharacterObject(player, playerCharacterObject);
            }
        }
    }
}