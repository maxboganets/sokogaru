using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sokogaru.Lobby
{
    public class UIPlayer : MonoBehaviour
    {
        [SerializeField] Text text;
        [SerializeField] Image image;
        Player player;

        public void SetPlayer(Player player)
        {
            this.player = player;
            var prefab = UILobby.instance.syncPlayersPrefabs[player.characterIndex];
            image.GetComponent<Image>().sprite = prefab.GetComponent<CharacterPersonalization>().getCharacterImage();
            text.text = player.characterName;
        }
    }
}