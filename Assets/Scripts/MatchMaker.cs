using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

namespace Sokogaru.Lobby
{
    public enum GameModeType
    {
        duel,
        brawl,
        captuteTheFlag
    };

    [System.Serializable]
    public class GameModeController
    {
        public GameModeController instance;
        public GameModeType mode;
        public int maxPlayers;

        public GameModeController(GameModeType mode)
        {
            this.mode = mode;
            this.instance = this;
            switch (this.mode)
            {
                case GameModeType.duel:
                    this.maxPlayers = 2;
                    break;
                case GameModeType.brawl:
                    this.maxPlayers = 4;
                    break;
                case GameModeType.captuteTheFlag:
                    this.maxPlayers = 4;
                    break;
            }
        }

        public GameModeController() { }
    }

    [System.Serializable]
    public class Match
    {
        public string matchID;

        public GameModeController gameModeController;
        public bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public string sceneName;

        public SyncListGameObject players = new SyncListGameObject();

        public Match(string matchID, GameObject player, GameModeType gameMode)
        {
            this.matchID = matchID;
            this.gameModeController = new GameModeController(gameMode);
            players.Add(player);
        }

        public Match () {}
    }

    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> { }

    [System.Serializable]
    public class SyncListMatch : SyncList<Match> { }

    public class MatchMaker : NetworkBehaviour
    {
        public static MatchMaker instance;
        public SyncListMatch matches = new SyncListMatch();
        public SyncList<string> matchIDs = new SyncList<string>();

        [Header("Duel Mode")]
        [SerializeField] GameObject duelGameManagerPrefab;
        public List<string> duelSceneNames;

        public static int matchIDLength = 5;

        void Start()
        {
            instance = this;
        }

        private string getRandomSceneNameForMode(GameModeType _gameMode)
        {
            List<string> sceneNamesArray = new List<string>();
            switch (_gameMode)
            {
                case GameModeType.duel:
                    sceneNamesArray = this.duelSceneNames;
                    break;
            }
            return sceneNamesArray[Random.Range(0, sceneNamesArray.Count)];
        }

        public bool HostGame(string _matchID, GameObject _player, bool publicMatch, out int playerIndex)
        {
            playerIndex = -1;
            GameModeType gameMode = GameModeType.duel;
            if (!matchIDs.Contains(_matchID))
            {
                this.matchIDs.Add(_matchID);
                Match match = new Match(_matchID, _player, gameMode);
                match.publicMatch = publicMatch;
                match.sceneName = this.getRandomSceneNameForMode(gameMode);
                this.matches.Add(match);
                Debug.Log($"Match generated");
                playerIndex = 1;
                return true;
            } else
            {
                Debug.Log($"Match ID already exists");
                return false;
            }
        }

        public bool JoinGame(string _matchID, GameObject _player, out int playerIndex)
        {
            playerIndex = -1;
            if (matchIDs.Contains(_matchID))
            {
                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].matchID == _matchID)
                    {
                        matches[i].players.Add(_player);
                        playerIndex = matches[i].players.Count;
                        // If match fullfilled - start match
                        if (matches[i].players.Count == matches[i].gameModeController.maxPlayers)
                        {
                            // Set match.inMatch = true
                            matches[i].inMatch = true;
                            // Begin game
                            this.BeginGame(_matchID);
                        }
                        break;
                    }
                }
                Debug.Log($"Match joined");
                return true;
            }
            else
            {
                Debug.Log($"Match ID does not exist");
                return false;
            }
        }

        public bool SearchGame(GameObject _player, out int playerIndex, out string matchID)
        {
            playerIndex = -1;
            matchID = string.Empty;

            for (int i = 0; i < matches.Count; i++ )
            {
                if (matches[i].publicMatch && !matches[i].matchFull && !matches[i].inMatch)
                {
                    matchID = this.matches[i].matchID;
                    if (this.JoinGame(matchID, _player, out playerIndex))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void BeginGame(string _matchID)
        {
            // Allow all clients to connect and initiate all classes
            StartCoroutine(this._BeginGameAfterWait(_matchID));
        }

        private void InstantiateGameManager(Match currentMatch)
        {
            GameObject newGameManager = null;
            switch (currentMatch.gameModeController.mode)
            {
                case GameModeType.duel:
                    newGameManager = Instantiate(this.duelGameManagerPrefab);
                    break;
            }
            NetworkServer.Spawn(newGameManager);
            newGameManager.GetComponent<NetworkMatchChecker>().matchId = currentMatch.matchID.ToGuid();
            var gameManager = newGameManager.GetComponent<DuelModeManager>();
            switch (currentMatch.gameModeController.mode)
            {
                case GameModeType.duel:
                    gameManager =  newGameManager.GetComponent<DuelModeManager>();
                    break;
            }
            // Init Game for all the players and load scene
            foreach (var player in currentMatch.players)
            {
                Player _player = player.GetComponent<Player>();
                gameManager.AddPlayer(_player);
                _player.StartGame(currentMatch.sceneName);
            }
            gameManager.StartMatch();
        }

        private IEnumerator _BeginGameAfterWait(string _matchID)
        {
            yield return new WaitForSeconds(0.3F);

            Match currentMatch = null;
            for (int i = 0; i < this.matches.Count; i++)
            {
                if (this.matches[i].matchID == _matchID)
                {
                    currentMatch = this.matches[i];
                }
            }
            if (currentMatch != null)
            {
                this.InstantiateGameManager(currentMatch);
            }
        }

        public static string GetRandomMatchId()
        {
            string _id = string.Empty;
            for (int i = 0; i < matchIDLength; i++)
            {
                int random = Random.Range(0, 36);
                if (random < 26)
                {
                    _id += (char)(random + 65);
                } else
                {
                    _id += (random - 26).ToString();
                }
            }
            Debug.Log($"Random Match ID: {_id}");
            return _id;
        }

        public void PlayerDisconnected(Player player, string _matchID)
        {
            for (int i = 0; i < this.matches.Count; i++)
            {
                if (this.matches[i].matchID == _matchID)
                {
                    int playerIndex = this.matches[i].players.IndexOf(player.gameObject);
                    this.matches[i].players.RemoveAt(playerIndex);
                    Debug.Log($"Player disconected from match {_matchID} | {this.matches[i].players.Count} players remain");
                    if (this.matches[i].players.Count == 0)
                    {
                        Debug.Log($"No more players in match. Terminating {_matchID}");
                        this.matches.RemoveAt(i);
                        this.matchIDs.Remove(_matchID);
                    }
                    break;
                }
            }
        }

        public void EndGame()
        {

        }
    }
}

public static class MatchExtensions
{
    public static System.Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new System.Guid(hashBytes);
    }
}