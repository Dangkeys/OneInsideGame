using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] public CharacterDatabase CharactersCollection;

    public static CharacterDatabase CharactersCollectionStatic;

    private void Awake()
    {
        CharactersCollectionStatic = CharactersCollection;
        CharactersCollectionStatic.Initialize();
    }

    //--------------------------------------
    // Static Method
    //--------------------------------------

    static public GameObject GetCharacterSkin(GameObject targetCharacter)
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

    //--------------------------------------
    // Change Character
    //--------------------------------------

    [ServerRpc]
    public static void ChangeCharacterServerRpc(string character, Character.SearchType searchType = Character.SearchType.Default, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log(serverRpcParams.Receive.SenderClientId);
        ChangeCharacterClientRpc(serverRpcParams.Receive.SenderClientId, character, searchType);
    }

    [ClientRpc]
    public static void ChangeCharacterClientRpc(ulong clientId, string character, Character.SearchType searchType)
    {
        Debug.Log(clientId);
        Player targetPlayer = PlayerManager.GetPlayerByClientId(clientId);
        CharacterSO characterSO = CharactersCollectionStatic.GetCharacter(character, searchType);
        if (characterSO == null)
        {
            Debug.LogError($"Character '{character}' not found");
            return;
        }
        Animator animator = targetPlayer.GetComponent<Animator>();
        ChangeChracter(targetPlayer.PlayerVisual, characterSO, animator);
    }

    public static void ChangeChracter(GameObject targetPlayerVisual, CharacterSO characterSO, Animator animator = null)
    {
        GameObject currentPlayerSkin = GetCharacterSkin(targetPlayerVisual);
        GameObject targetPlayerSkin = characterSO.CharacterSkin;
        if (animator)
        {
            animator.avatar = characterSO.CharacterAvatar;
        }

        SkinnedMeshRenderer currentPlayerSkinRenderer = currentPlayerSkin.GetComponent<SkinnedMeshRenderer>(),
         targetPlayerSkinRenderer = targetPlayerSkin.GetComponent<SkinnedMeshRenderer>();

        currentPlayerSkinRenderer.sharedMesh = targetPlayerSkinRenderer.sharedMesh;
        currentPlayerSkinRenderer.sharedMaterials = targetPlayerSkinRenderer.sharedMaterials;
    }
}
