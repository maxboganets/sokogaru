using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Sokogaru.Lobby
{
    public class LobbyPlayer : NetworkBehaviour
    {
        public static LobbyPlayer localPlayer;

        [SyncVar]
        public string matchID;

        NetworkMatchChecker networkMatchChecker;

        void Start()
        {
            if (isLocalPlayer)
            {
                localPlayer = this;
            }

            networkMatchChecker = GetComponent<NetworkMatchChecker>();
        }

        public void HostGame()
        {
            string matchID = MatchMaker.GetRandomMatchId();
            this.CmdHostGame(matchID);
        }

        [Command]
        void CmdHostGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame(_matchID, gameObject))
            {
                Debug.Log($"<color = green>Game Hosted Successfully</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                this.TargetHostGame(true, _matchID);
            } else
            {
                Debug.Log($"<color = red>Game Hosted Failed</color>");
                this.TargetHostGame(false, _matchID);
            }
        }

        [TargetRpc]
        void TargetHostGame(bool success, string _matchID)
        {
            Debug.Log($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.HostSuccess(success);
        }
    }
}