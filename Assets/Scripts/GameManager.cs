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

        public void AssignCharacterObject(Player player, GameObject characterObject)
        {
            player.characterObject = characterObject;
        }

        public abstract void StartMatch();
    }
}