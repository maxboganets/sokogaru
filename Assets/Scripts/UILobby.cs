using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Mirror;

namespace Sokogaru.Lobby
{
    public class UILobby : MonoBehaviour
    {
        public static UILobby instance;

        [Header("Lobby Container")]
        [SerializeField] Canvas sceneUICanvas;

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

        public void EnableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = true;
        }

        public void DisableSceneUICanvas()
        {
            this.sceneUICanvas.enabled = false;
        }

        void Start()
        {
            instance = this;
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