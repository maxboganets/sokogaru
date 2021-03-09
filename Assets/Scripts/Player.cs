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
        //[SyncVar] public GameObject characterPrefab;
        [SyncVar] public int characterIndex;
        [SyncVar] public string characterName;

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
            DontDestroyOnLoad(gameObject);
        }

        /*
         * Select Character & Set Name
         */

        public void SetCharacter(int _characterIndex, string _characterName)
        {
            this.characterIndex = _characterIndex;
            this.characterName = _characterName;
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
                this.TargetHostGame(true, _matchID, playerIndex);
            }
            else
            {
                Debug.Log($"<color = red>Game Hosted Failed</color>");
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