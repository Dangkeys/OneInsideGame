using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterManager : NetworkBehaviour
{
    [field: SerializeField] public GameObject PlayerVisual { get; private set; }
    [field: SerializeField] public CharacterDatabase ChractersCollection { get; private set; }

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
    public void ChangeCharacterServerRpc(string character, Character.SearchType searchType = Character.SearchType.Name, ServerRpcParams serverRpcParams = default)
    {
        ChangeCharacterClientRpc(serverRpcParams.Receive.SenderClientId, character, searchType);
    }

    [ClientRpc]
    public void ChangeCharacterClientRpc(ulong clientId, string character, Character.SearchType searchType = Character.SearchType.Name)
    {
        Player targetPlayer = PlayerManager.GetPlayerByClientId(clientId);
        CharacterSO characterSO = ChractersCollection.GetCharacterBySearchType(character, searchType);
        if (characterSO == null)
        {
            Debug.LogError($"Character '{character}' not found");
            return;
        }
        ChangeChracter(targetPlayer, characterSO);
    }

    public void ChangeChracter(Player targetPlayer, CharacterSO characterSO)
    {
        GameObject currentPlayerSkin = GetCharacterSkin(targetPlayer.PlayerVisual);
        Animator animator = targetPlayer.GetComponent<Animator>();

        GameObject targetPlayerSkin = characterSO.CharacterSkin;
        animator.avatar = characterSO.CharacterAvatar;

        SkinnedMeshRenderer currentPlayerSkinRenderer = currentPlayerSkin.GetComponent<SkinnedMeshRenderer>(),
         targetPlayerSkinRenderer = targetPlayerSkin.GetComponent<SkinnedMeshRenderer>();

        currentPlayerSkinRenderer.sharedMesh = targetPlayerSkinRenderer.sharedMesh;
        currentPlayerSkinRenderer.sharedMaterials = targetPlayerSkinRenderer.sharedMaterials;
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
