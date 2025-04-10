using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.WSA;

public class CharacterManager : NetworkBehaviour
{
    [SerializeField] public CharactersDatabase AllCharactersDatabase;
    [SerializeField] public CharacterSO DefaultCrewmateCharacter;
    [SerializeField] public CharacterSO DefaultImposterCharacter;


    private void Awake()
    {
        AllCharactersDatabase.Initialize();
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

    static public GameObject GetCharacterRoot(GameObject targetCharacter)
    {
        foreach (Transform child in targetCharacter.transform)
        {
            if (child.name == "Root")
            {
                return child.gameObject;
            }
        }
        return null;
    }

    static public CharacterSO GetCharacterSO(string character, Character.SearchType searchType = Character.SearchType.Default)
    {
        return OneInsideGameManager.Instance.CharacterManager.AllCharactersDatabase.GetCharacter(character, searchType);
    }

    public static void ChangeChracter(GameObject targetPlayerVisual, CharacterSO characterSO, Animator animator = null)
    {
        targetPlayerVisual.transform.localScale = characterSO.CharacterVisual.transform.localScale;

        GameObject currentPlayerSkin = GetCharacterSkin(targetPlayerVisual);
        GameObject currentPlayerRoot = GetCharacterRoot(targetPlayerVisual);

        GameObject targetPlayerSkin = characterSO.CharacterSkin;
        GameObject targetRoot = characterSO.CharacterRoot;

        string currentSkinName = currentPlayerSkin.name;
        string currentRootName = currentPlayerRoot.name;

        if (animator)
        {
            Dictionary<string, bool> savedAllParameters = new Dictionary<string, bool>();
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    bool value = animator.GetBool(param.name);
                    savedAllParameters.Add(param.name, value);
                }
            }
            animator.avatar = characterSO.CharacterAvatar;

            foreach (var item in savedAllParameters)
            {
                animator.SetBool(item.Key, item.Value);
            }
        }

        if (currentPlayerSkin != null)
        {
            GameObject.Destroy(currentPlayerSkin);
        }
        GameObject newSkin = GameObject.Instantiate(targetPlayerSkin, targetPlayerVisual.transform);
        newSkin.name = currentSkinName;

        if (currentPlayerRoot != null)
        {
            GameObject.Destroy(currentPlayerRoot);
        }
        GameObject newRoot = GameObject.Instantiate(targetRoot, targetPlayerVisual.transform);
        newRoot.name = currentRootName;
    }

    public static void ChangeChracter(GameObject targetPlayerVisual, string characterName, Animator animator = null)
    {
        CharacterSO characterSO = GetCharacterSO(characterName);
        ChangeChracter(targetPlayerVisual, characterSO, animator);
    }

    //--------------------------------------
    // Change Character
    //--------------------------------------

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCharacterServerRpc(string character, Character.SearchType searchType = Character.SearchType.Default, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        ChangeCharacterClientRpc(senderClientId, character, searchType);
    }

    [ClientRpc]
    public void ChangeCharacterClientRpc(ulong clientId, string character, Character.SearchType searchType)
    {
        Player targetPlayer = PlayerManager.GetPlayerByClientId(clientId);
        CharacterSO characterSO = GetCharacterSO(character, searchType);
        if (characterSO == null)
        {
            Debug.LogError($"Character '{character}' not found");
            return;
        }
        Animator animator = targetPlayer.GetComponent<Animator>();
        ChangeChracter(targetPlayer.PlayerVisual, characterSO, animator);
    }
}
