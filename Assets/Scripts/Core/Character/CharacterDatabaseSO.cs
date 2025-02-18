using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    [field: SerializeField]  public List<CharacterSO> Characters;

    public CharacterSO GetCharacterById(string id)
    {
        return Characters.Find(character => character.ID == id);
    }
}
