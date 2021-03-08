using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Sokogaru.Lobby
{
    public class CharacterSelect : NetworkBehaviour
    {
        [Header("UI Components")]
        public Image characterImage;
        public Text characterName;
        public InputField playerName;
        public Button submitButton;

        private int selectedCharacterIndex;
        private int minNickNameLength = 3;

        void Awake()
        {
            if (UILobby.instance.syncPlayersPrefabs != null && UILobby.instance.syncPlayersPrefabs.Count > 0)
            {
                this.SetCharacter(0);
            }
            this.submitButton.enabled = false;
        }

        private void Update()
        {
            this.submitButton.enabled = (this.playerName.text.Length >= this.minNickNameLength) ? true : false;
        }

        public void ScrollLeft()
        {
            var newIndex = (this.selectedCharacterIndex > 0)
                ? this.selectedCharacterIndex - 1
                : UILobby.instance.syncPlayersPrefabs.Count - 1;
            this.SetCharacter(newIndex);
        }

        public void ScrollRight()
        {
            var newIndex = (this.selectedCharacterIndex < UILobby.instance.syncPlayersPrefabs.Count - 1)
                ? this.selectedCharacterIndex + 1
                : 0;
            this.SetCharacter(newIndex);
        }

        private void SetCharacter(int _characterIndex)
        {
            this.selectedCharacterIndex = _characterIndex;
            var selectedPrefab = UILobby.instance.syncPlayersPrefabs[this.selectedCharacterIndex];
            this.characterImage.GetComponent<Image>().sprite = selectedPrefab.GetComponent<CharacterPersonalization>().getCharacterImage();
            this.characterName.text = selectedPrefab.GetComponent<CharacterPersonalization>().getCharacterName();
        }

        public void SubmitCharacter()
        {
            Player.localPlayer.SetCharacter(this.selectedCharacterIndex, this.playerName.text);
            UILobby.instance.enableHostCanvas();
        }
    }
}