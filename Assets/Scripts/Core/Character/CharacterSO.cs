
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public string CharacterBIO { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }

    [field: SerializeField] public GameObject CharacterVisual { get; private set; }

    public GameObject CharacterSkin { get; private set; }
    public GameObject CharacterRoot { get; private set; }
    public Avatar CharacterAvatar { get; private set; }

    private string privateId;
    public string ID
    {
        get
        {
            if (string.IsNullOrEmpty(privateId))
            {
                privateId = GameUtilities.GenerateID("Character", CharacterName);
            }
            return privateId;
        }
    }

    public void Initialize()
    {
        if (CharacterName == null || CharacterName == "")
        {
            CharacterName = name;
        }

        if (CharacterBIO == null || CharacterBIO == "")
        {
            CharacterBIO = "Hi, I'm " + CharacterName;
        }

        if (CharacterVisual != null)
        {
            CharacterVisual.name = CharacterName;

            // Setup character skin
            CharacterSkin = CharacterManager.GetCharacterSkin(CharacterVisual);
            CharacterRoot = CharacterManager.GetCharacterRoot(CharacterVisual);
            CharacterAvatar = CharacterVisual.GetComponent<Animator>()?.avatar;
        }
    }
}
