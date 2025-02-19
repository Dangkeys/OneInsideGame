using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
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

    public CharacterSO GetCharacterBySearchType(string search, Character.SearchType searchType = Character.SearchType.Name)
    {
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
}
