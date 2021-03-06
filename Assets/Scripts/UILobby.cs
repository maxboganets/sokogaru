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

        [Header("Host Join")]
        [SerializeField] InputField joinMatchInput;
        [SerializeField] Button joinButton;
        [SerializeField] Button hostButton;
        [SerializeField] Canvas lobbyCanvas;

        [Header("Lobby")]
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        void Start()
        {
            instance = this;
            DontDestroyOnLoad(instance);
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

        public void JoinSuccess(bool success)
        {
            if (success)
            {
                lobbyCanvas.enabled = true;
                this.SpawnPlayerUIPrefab(Player.localPlayer);
                matchIDText.text = Player.localPlayer.matchID;
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
            GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
            newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        }

        public void BeginGame()
        {
            Player.localPlayer.BeginGame();
        }
    }
}