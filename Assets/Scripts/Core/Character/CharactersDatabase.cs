using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersDatabase", menuName = "Scriptable Objects/CharactersDatabase")]
public class CharactersDatabase : ScriptableObject
{
    [field: SerializeField] public List<CharactersCollection> CharactersCollections;

    public CharacterSO GetCharacter(string search, Character.SearchType searchType)
    {
        foreach (CharactersCollection charactersCollection in CharactersCollections)
        {
            if (charactersCollection == null)
            {
                Debug.LogWarning("CharactersCollection is null, skipping...");
                continue;
            }

            CharacterSO characterSO = charactersCollection.GetCharacter(search, searchType);
            if (characterSO != null)
            {
                return characterSO;
            }
        }
        Debug.Log("Can't find character : " + search);
        return null;
    }

    public void Initialize()
    {
        foreach (CharactersCollection charactersCollection in CharactersCollections)
        {
            charactersCollection.Initialize();
        }
    }
}