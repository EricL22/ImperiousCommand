using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    private Dictionary<string, Character> activeCharacters = new Dictionary<string, Character>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public Character CreateCharacter(CharacterData data)
    {
        Character character = new Character();
        character.data = data;
        activeCharacters[data.characterId] = character;
        return character;
    }

    public Character GetCharacterById(string id)
    {
        return activeCharacters.TryGetValue(id, out Character character) ? character : null;
    }
}
