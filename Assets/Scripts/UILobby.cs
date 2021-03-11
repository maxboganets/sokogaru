using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Sokogaru.Lobby
{
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
        [SerializeField] List<Selectable> lobbySelectables = new List<Selectable>();
        [SerializeField] Canvas lobbyCanvas;
        [SerializeField] Canvas searchCanvas;


        [Header("Lobby")]
        [SerializeField] Transform UIPlayersContainer;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        GameObject playerLobbyUI;

        bool searching = false;

        public void EnableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = true;
        }

        public void DisableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = false;
        }

        public void DisableAllCanvases()
        {
            this.characterSelectCanvas.enabled = false;
            this.lobbyCanvas.enabled = false;
            this.searchCanvas.enabled = false;
        }

        public void EnableHostCanvas()
        {
            this.EnableSceneUICanvas();
            this.DisableAllCanvases();
        }

        public void EnableSearchCanvas()
        {
            this.EnableSceneUICanvas();
            this.DisableAllCanvases();
            this.searchCanvas.enabled = true;
        }

        public void EnableCharacterSelectCanvas()
        {
            this.EnableSceneUICanvas();
            this.DisableAllCanvases();
            this.characterSelectCanvas.enabled = true;
        }

        public void EnableLobbyCanvas()
        {
            this.EnableSceneUICanvas();
            this.DisableAllCanvases();
            this.lobbyCanvas.enabled = true;
        }

        void Start()
        {
            instance = this;
            this.EnableCharacterSelectCanvas();
        }

        public void HostPrivate()
        {
            lobbySelectables.ForEach(x => x.interactable = false);
            Player.localPlayer.HostGame(false);
        }

        public void HostPublic()
        {
            lobbySelectables.ForEach(x => x.interactable = false);
            Player.localPlayer.HostGame(true);
        }

        public void HostSuccess(bool success)
        {
            if (success)
            {
                this.EnableLobbyCanvas();
                if (this.playerLobbyUI != null)
                {
                    Destroy(this.playerLobbyUI);
                }
                this.playerLobbyUI = this.SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = Player.localPlayer.matchID;
                beginGameButton.SetActive(true);
            } else
            {
                lobbySelectables.ForEach(x => x.interactable = true);
            }
        }

        public void Join()
        {
            joinMatchInput.interactable = false;
            lobbySelectables.ForEach(x => x.interactable = false);

            Player.localPlayer.JoinGame(joinMatchInput.text.ToUpper());
        }

        public void JoinSuccess(bool success, string matchID)
        {
            if (success)
            {
                this.EnableLobbyCanvas();
                this.beginGameButton.SetActive(false);
                if (this.playerLobbyUI != null)
                {
                    Destroy(this.playerLobbyUI);
                }
                this.playerLobbyUI = this.SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = matchID;
            }
            else
            {
                lobbySelectables.ForEach(x => x.interactable = true);
            }
        }

        public void DeleteAllPlayerUIPrefabs()
        {
            foreach (Transform child in UIPlayersContainer)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public GameObject SpawnPlayerUIPrefab(Player player)
        {
            GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayersContainer);
            newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
            newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
            return newUIPlayer;
        }

        public void BeginGame()
        {
            Player.localPlayer.BeginGame();
        }

        public void SearchGame()
        {
            Debug.Log($"Searching for game");
            this.EnableSearchCanvas();
            StartCoroutine(this.SearchingForGame());
        }

        IEnumerator SearchingForGame()
        {
            this.searching = true;
            float currentTime = 1;
            while(this.searching)
            {
                if (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                } else
                {
                    currentTime = 1;
                    Player.localPlayer.SearchGame();
                }
                yield return null;
            }
        }

        public void SearchSuccess(bool success, string matchID)
        {
            if (success)
            {
                this.searching = false;
                this.JoinSuccess(success, matchID);
                this.EnableLobbyCanvas();
            }
        }

        public void SearchCancel()
        {
            this.searching = false;
            this.EnableHostCanvas();
            lobbySelectables.ForEach(x => x.interactable = true);
        }

        public void DisconnectLobby()
        {
            if (this.playerLobbyUI != null)
            {
                Destroy(this.playerLobbyUI);
            }
            Player.localPlayer.DisconectGame();
            lobbySelectables.ForEach(x => x.interactable = true);
            this.beginGameButton.SetActive(false);
            this.EnableHostCanvas();
        }
    }
}