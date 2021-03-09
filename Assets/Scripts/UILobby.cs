using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Sokogaru.Lobby
{
    [System.Serializable]
    public class SyncListPlayersPrefabs : SyncList<GameObject> { }

    public class UILobby : NetworkBehaviour
    {
        public static UILobby instance;

        [Header("Lobby Container")]
        [SerializeField] Canvas sceneUICanvas;

        [Header("Character Select")]
        [SerializeField] Canvas characterSelectCanvas;
        public GameObject[] charactersPrefabs;

        [Header("Host Join")]
        [SerializeField] InputField joinMatchInput;
        [SerializeField] Button joinButton;
        [SerializeField] Button hostButton;
        [SerializeField] Canvas lobbyCanvas;

        [Header("Lobby")]
        [SerializeField] Transform UIPlayersContainer;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        public SyncListPlayersPrefabs syncPlayersPrefabs;

        public void EnableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = true;
        }

        public void DisableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = false;
        }

        public void enableHostCanvas()
        {
            this.EnableSceneUICanvas();
            this.characterSelectCanvas.enabled = false;
            this.lobbyCanvas.enabled = false;
        }

        public void EnableCharacterSelectCanvas()
        {
            this.EnableSceneUICanvas();
            this.characterSelectCanvas.enabled = true;
            this.lobbyCanvas.enabled = false;
        }

        public void EnableLobbyCanvas()
        {
            this.EnableSceneUICanvas();
            this.characterSelectCanvas.enabled = false;
            this.lobbyCanvas.enabled = true;
        }

        void Start()
        {
            instance = this;
            this.lobbyCanvas.enabled = false;
            this.characterSelectCanvas.enabled = true;

            this.syncPlayersPrefabs = new SyncListPlayersPrefabs();
            for (int i = 0; i < this.charactersPrefabs.Length; i++)
            {
                this.syncPlayersPrefabs.Add(this.charactersPrefabs[i]);
            }
        }

        public void Host()
        {
            joinMatchInput.interactable = false;
            joinButton.interactable = false;
            hostButton.interactable = false;

            Player.localPlayer.HostGame();
        }

        public void HostSuccess(bool success)
        {
            if (success)
            {
                lobbyCanvas.enabled = true;
                this.SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = Player.localPlayer.matchID;
                beginGameButton.SetActive(true);
            } else
            {
                joinMatchInput.interactable = true;
                joinButton.interactable = true;
                hostButton.interactable = true;
            }
        }

        public void Join()
        {
            joinMatchInput.interactable = false;
            joinButton.interactable = false;
            hostButton.interactable = false;

            Player.localPlayer.JoinGame(joinMatchInput.text.ToUpper());
        }

        public void JoinSuccess(bool success, string matchID)
        {
            if (success)
            {
                lobbyCanvas.enabled = true;
                this.SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = matchID;
            }
            else
            {
                joinMatchInput.interactable = true;
                joinButton.interactable = true;
                hostButton.interactable = true;
            }
        }

        public void DeleteAllPlayerUIPrefabs()
        {
            foreach (Transform child in UIPlayersContainer)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void SpawnPlayerUIPrefab(Player player)
        {
            GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayersContainer);
            newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
            newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
        }

        public void BeginGame()
        {
            Player.localPlayer.BeginGame();
        }
    }
}