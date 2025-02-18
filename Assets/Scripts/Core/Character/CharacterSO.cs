
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [field: SerializeField] public string ID {get; private set;}
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public string CharacterBIO { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }

    [field: SerializeField] public GameObject PlayerObject {get; private set;}
}
