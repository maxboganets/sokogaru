using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Sokogaru.Lobby
{
    public class GameManager : NetworkBehaviour
    {
        public SyncList<Player> players = new SyncList<Player>();

        public void AddPlayer(Player _player)
        {
            this.players.Add(_player);
        }

        public void SpawnPlayers()
        {
            this.ClientSpawnPlayers();
        }

        //[ClientRpc]
        void ClientSpawnPlayers()
        {
            Debug.Log($"<color=red>>>>>>>>> PLAYERS: </color>" + this.players.Count);
            foreach (var player in this.players)
            {
                Debug.Log("> player: " + player.characterName);
                var prefab = UILobby.instance.charactersPrefabs[player.characterIndex];
                //var playerInput = (GameObject)Instantiate(
                //    prefab,
                //    Vector3.zero,
                //    Quaternion.identity
                //);
                //playerInput.GetComponent<Rigidbody2D>().transform.position = GameObject.Find("Player" + player.characterIndex + "Spawn").transform.position;
                GameObject spawnPointInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                NetworkServer.Spawn(spawnPointInstance);

            }
        }
    }
}