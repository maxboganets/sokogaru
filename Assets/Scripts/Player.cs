using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

namespace Sokogaru.Lobby
{
    public class Player : NetworkBehaviour
    {
        public static Player localPlayer;

        [SyncVar] public string matchID;
        [SyncVar] public int playerIndex;
        [SyncVar] public int characterIndex;
        [SyncVar] public string characterName;

        NetworkMatchChecker networkMatchChecker;
        GameObject playerLobbyUI;

        void Awake()
        {
            this.networkMatchChecker = GetComponent<NetworkMatchChecker>();
            //DontDestroyOnLoad(gameObject);
        }

        public override void OnStartClient()
        {
            if (isLocalPlayer)
            {
                localPlayer = this;
            }
            else
            {
                Debug.Log($"Spawning other player UI");
                this.playerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab(this);
            }
        }

        public override void OnStopClient()
        {
            Debug.Log($"Client stopped");
            this.ServerDisconnect();
        }

        public override void OnStopServer()
        {
            Debug.Log($"Client stopped on server");
            this.ClientDisconnect();
        }

        /*
         * Select Character & Set Name
         */

        [Command]
        public void SetCharacter(int _characterIndex, string _characterName)
        {
            this.characterIndex = _characterIndex;
            this.characterName = _characterName;
        }

        /*
         * Host Match
         */

        public void HostGame(bool publicMatch)
        {
            string matchID = MatchMaker.GetRandomMatchId();
            this.CmdHostGame(matchID, publicMatch);
        }

        [Command]
        void CmdHostGame(string _matchID, bool publicMatch)
        {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame(_matchID, gameObject, publicMatch, out playerIndex))
            {
                Debug.Log($"<color=green>Game Hosted Successfully</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                this.TargetHostGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color=red>Game Hosted Failed</color>");
                this.TargetHostGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetHostGame(bool success, string _matchID, int _playerIndex)
        {
            this.playerIndex = _playerIndex;
            Debug.Log($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.HostSuccess(success);
        }

        /*
         * Join Match
         */

        public void JoinGame(string _inputID)
        {
            this.CmdJoinGame(_inputID, this);
        }

        [Command]
        void CmdJoinGame(string _matchID, Player _player)
        {
            matchID = _matchID;
            if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
            {
                Debug.Log($"<color=green>Game Joined Successfully</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                this.TargetJoinGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color = red>Game Joined Failed</color>");
                this.TargetJoinGame(false, _matchID, playerIndex);
            }
        }

        [TargetRpc]
        void TargetJoinGame(bool success, string _matchID, int _playerIndex)
        {
            this.playerIndex = _playerIndex;
            Debug.Log($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.JoinSuccess(success, _matchID);
        }

        /*
         * Search Match
         */

        public void SearchGame()
        {
            this.CmdSearchGame();
        }

        [Command]
        public void CmdSearchGame()
        {
            if (MatchMaker.instance.SearchGame(gameObject, out playerIndex, out matchID))
            {
                Debug.Log($"<color=green>Game Found</color>");
                networkMatchChecker.matchId = matchID.ToGuid();
                this.TargetSearchGame(true, matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color=red>Game Not Found</color>");
                this.TargetSearchGame(false, matchID, playerIndex);
            }
        }

        [TargetRpc]
        public void TargetSearchGame(bool success, string _matchID, int _playerIndex)
        {
            this.playerIndex = _playerIndex;
            Debug.Log($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.SearchSuccess(success, _matchID);
        }

        /*
         * Begin Match
         */

        public void BeginGame()
        {
            this.CmdBeginGame();
        }

        [Command]
        void CmdBeginGame()
        {
            MatchMaker.instance.BeginGame(matchID );
            Debug.Log($"<color=green>Game Beginning</color>");
        }

        public void StartGame()
        {
            this.TargetBeginGame();
        }

        [TargetRpc]
        void TargetBeginGame()
        {
            Debug.Log($"MatchID: {matchID} | Beginning");
            // Additively Load Game Scene
            SceneManager.LoadScene(2, LoadSceneMode.Additive);
            // Hide Lobby Canvas
            UILobby.instance.DisableSceneUICanvas();
        }

        /*
         * Disconnect Match
         */

        public void DisconectGame()
        {
            this.CmdDisconectGame();
        }

        [Command]
        void CmdDisconectGame()
        {
            this.ServerDisconnect();
        }

        void ServerDisconnect()
        {
            MatchMaker.instance.PlayerDisconnected(this, this.matchID);
            this.networkMatchChecker.matchId = string.Empty.ToGuid();
            this.RpcDisconectGame();
        }

        [ClientRpc]
        void RpcDisconectGame()
        {
            this.ClientDisconnect();
        }

        void ClientDisconnect()
        {
            if (this.playerLobbyUI != null)
            {
                Destroy(this.playerLobbyUI);
            }
        }
    }
}