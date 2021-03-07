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

        NetworkMatchChecker networkMatchChecker;

        void Start()
        {
            networkMatchChecker = GetComponent<NetworkMatchChecker>();

            if (isLocalPlayer)
            {
                localPlayer = this;
            } else
            {
                UILobby.instance.SpawnPlayerUIPrefab(this);
            }
        }

        void Awake()
        {
            Debug.Log("Player Awaken");
            DontDestroyOnLoad(gameObject);
        }

        /*
         * Host Match
         */

        public void HostGame()
        {
            string matchID = MatchMaker.GetRandomMatchId();
            this.CmdHostGame(matchID);
        }

        [Command]
        void CmdHostGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.HostGame(_matchID, gameObject, out playerIndex))
            {
                Debug.Log($"<color = green>Game Hosted Successfully</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                this.TargetHostGame(true, _matchID);
            }
            else
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

        /*
         * Join Match
         */

        public void JoinGame(string _inputID)
        {
            this.CmdJoinGame(_inputID);
        }

        [Command]
        void CmdJoinGame(string _matchID)
        {
            matchID = _matchID;
            if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
            {
                Debug.Log($"<color = green>Game Joined Successfully</color>");
                networkMatchChecker.matchId = _matchID.ToGuid();
                this.TargetJoinGame(true, _matchID);
            }
            else
            {
                Debug.Log($"<color = red>Game Joined Failed</color>");
                this.TargetJoinGame(false, _matchID);
            }
        }

        [TargetRpc]
        void TargetJoinGame(bool success, string _matchID)
        {
            Debug.Log($"MatchID: {matchID} == {_matchID}");
            UILobby.instance.JoinSuccess(success, _matchID);
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
            Debug.Log($"<color = green>Game Beginning</color>");
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
    }
}