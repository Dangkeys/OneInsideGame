using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CharactersCollection", menuName = "Scriptable Objects/CharactersCollection")]
public class CharactersCollection : ScriptableObject
{
    [field: SerializeField] public List<CharacterSO> Characters;

    public CharacterSO GetCharacterById(string id)
    {
        return Characters.Find(character => character.ID == id);
    }

    public CharacterSO GetCharacterByName(string name)
    {
        return Characters.Find(character => character.name == name);
    }

    public CharacterSO GetCharacter(string search, Character.SearchType searchType)
    {
        if (searchType == Character.SearchType.Default)
        {
            searchType = Character.SearchType.ID;
        }

        CharacterSO characterSO = null;
        switch (searchType)
        {
            case Character.SearchType.Name:
                characterSO = GetCharacterByName(search);
                break;
            case Character.SearchType.ID:
                characterSO = GetCharacterById(search);
                break;
        }
        return characterSO;
    }

    public void Initialize()
    {
        foreach (CharacterSO character in Characters)
        {
            character.Initialize();
        }
    }
}