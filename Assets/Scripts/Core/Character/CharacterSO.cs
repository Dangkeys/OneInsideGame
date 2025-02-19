
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public string CharacterBIO { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }

    [field: SerializeField] public GameObject CharacterVisual { get; private set; }

    public GameObject CharacterSkin { get; private set; }
    public Avatar CharacterAvatar { get; private set; }

    public void Initialize()
    {
        if (CharacterVisual != null)
        {
            CharacterVisual.name = CharacterName;

            // Setup character skin
            CharacterSkin = CharacterManager.GetCharacterSkin(CharacterVisual);
            CharacterAvatar = CharacterVisual.GetComponent<Animator>()?.avatar;
        }
    }
}
