using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPersonalization : MonoBehaviour
{
    [SerializeField] string characterName;
    [SerializeField] Sprite characterImage;

    public string getCharacterName()
    {
        return this.characterName;
    }

    public Sprite getCharacterImage()
    {
        return this.characterImage;
    }
}
