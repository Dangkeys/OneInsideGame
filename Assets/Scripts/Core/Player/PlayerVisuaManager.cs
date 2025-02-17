using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerVisuaManager : NetworkBehaviour
{
    [field: SerializeField] public GameObject CurrentPlayerVisual { get; private set; }

    //--------------------------------------
    // Private Variables
    //--------------------------------------

    private GameObject currentPlayerSkin;
    private Animator animator;

    void Awake()
    {
        currentPlayerSkin = GetCharacterSkin(CurrentPlayerVisual);
        animator = GetComponent<Animator>();
    }

    [ServerRpc]
    public void ChangeCharacterServerRpc(string characterName)
    {
        ChangeCharacterClientRpc(characterName);
    }

    [ClientRpc]
    public void ChangeCharacterClientRpc(string characterName)
    {
        CharacterSO characterSO = Resources.Load<CharacterSO>($"PlayerCharacters/{characterName}");
        GameObject targetPlayerSkin = GetCharacterSkin(characterSO.PlayerVisual);

        animator.avatar = characterSO.PlayerVisual.GetComponent<Animator>().avatar;

        SkinnedMeshRenderer currentPlayerSkinRenderer = currentPlayerSkin.GetComponent<SkinnedMeshRenderer>(),
         targetPlayerSkinRenderer = targetPlayerSkin.GetComponent<SkinnedMeshRenderer>();

        currentPlayerSkinRenderer.sharedMesh = targetPlayerSkinRenderer.sharedMesh;
        currentPlayerSkinRenderer.sharedMaterials = targetPlayerSkinRenderer.sharedMaterials;
    }

    public GameObject GetCharacterSkin(GameObject targetCharacter)
    {
        foreach (Transform child in targetCharacter.transform)
        {
            if (child.name != "Root")
            {
                return child.gameObject;
            }
        }
        return null;
    }

    // Test

    [field: SerializeField] public CharacterSO Test1 { get; private set; }
    [field: SerializeField] public CharacterSO Test2 { get; private set; }

    void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeCharacterServerRpc(Test1.name);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCharacterServerRpc(Test2.name);
        }
    }
}
