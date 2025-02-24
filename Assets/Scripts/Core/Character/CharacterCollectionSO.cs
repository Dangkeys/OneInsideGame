using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Scriptable Objects/CharacterCollectionSO")]
public class CharacterCollectionSO : ScriptableObject
{
    [field: SerializeField]  public List<CharacterSO> Characters;

    public CharacterSO GetCharacterById(string id)
    {
        return Characters.Find(character => character.ID == id);
    }
}
