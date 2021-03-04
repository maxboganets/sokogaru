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

        public static int matchIDLength = 5;

        void Start()
        {
            instance = this;
        }

        public bool HostGame(string _matchID, GameObject _player)
        {
            if (!matchIDs.Contains(_matchID))
            {
                this.matchIDs.Add(_matchID);
                this.matches.Add(new Match(_matchID, _player));
                Debug.Log($"Match generated");
                return true;
            } else
            {
                Debug.Log($"Match ID already exists");
                return false;
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