using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/Character")]
public class CharacterSO : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public string CharacterBIO { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }

    [field: SerializeField] public GameObject PlayerVisual { get; private set; }
    [field: SerializeField] public PlayerVisual.Type SizeType { get; private set; }

    [field: SerializeField] public float Width { get; private set; }
    [field: SerializeField] public float Height { get; private set; }
}
