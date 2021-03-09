using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Mirror;

namespace Sokogaru.Lobby
{
    [System.Serializable]
    public class Match
    {
        public string matchID;
        public SyncListGameObject players = new SyncListGameObject();

        public Match(string matchID, GameObject player)
        {
            this.matchID = matchID;
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

        [SerializeField] GameObject gameManagerPrefab;

        public static int matchIDLength = 5;

        void Start()
        {
            instance = this;
        }

        public bool HostGame(string _matchID, GameObject _player, out int playerIndex)
        {
            playerIndex = -1;
            if (!matchIDs.Contains(_matchID))
            {
                this.matchIDs.Add(_matchID);
                this.matches.Add(new Match(_matchID, _player));
                Debug.Log($"Match generated");
                playerIndex = 1;
                //Debug.Log("> Player: " + _player.GetComponent<Player>().characterName);
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


                        // Delete all players from lobby
                        UILobby.instance.DeleteAllPlayerUIPrefabs();
                        // Spawn all players in lobby
                        foreach (GameObject player in matches[i].players)
                        {
                            Debug.Log("spawn player: " + player.GetComponent<Player>().characterName);
                            UILobby.instance.SpawnPlayerUIPrefab(player.GetComponent<Player>());
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

        public void BeginGame(string _matchID)
        {
            GameObject newGameManager = Instantiate(this.gameManagerPrefab);
            NetworkServer.Spawn(newGameManager);
            newGameManager.GetComponent<NetworkMatchChecker>().matchId = _matchID.ToGuid();
            GameManager gameManager = newGameManager.GetComponent<GameManager>();

            for (int i = 0; i < this.matches.Count; i++) {
                if (this.matches[i].matchID == _matchID)
                {
                    foreach (var player in this.matches[i].players)
                    {
                        Player _player = player.GetComponent<Player>();
                        gameManager.AddPlayer(_player);
                        _player.StartGame();

                    }
                    break;
                }
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