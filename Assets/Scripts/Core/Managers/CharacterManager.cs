using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.WSA;
using System.Threading.Tasks;

public class CharacterManager : SingletonNetwork<CharacterManager>
{
    /*
    -------------------------------------------------------
    SERIALIZED FIELDS
    -------------------------------------------------------
    */
    [SerializeField] public CharactersDatabase AllCharactersDatabase;
    [SerializeField] public CharacterSO DefaultCrewmateCharacter;
    [SerializeField] public CharacterSO DefaultImposterCharacter;

    /*
    -------------------------------------------------------
    UNITY EVENTS
    -------------------------------------------------------
    */
    protected override void Awake()
    {
        base.Awake();
        AllCharactersDatabase.Initialize();
    }

    /*
    -------------------------------------------------------
    STATIC UTILITY METHODS
    -------------------------------------------------------
    */

    static public GameObject GetCharacterSkin(GameObject targetCharacter)
    {
        foreach (Transform child in targetCharacter.transform)
        {
            if (child.name != "Root" && child.name != "IK")
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



    public static async void ChangeChracter(GameObject targetPlayerVisual, CharacterSO characterSO, Animator animator = null)
    {
        targetPlayerVisual.transform.localScale = characterSO.CharacterVisual.transform.localScale;

        GameObject currentPlayerSkin = GetCharacterSkin(targetPlayerVisual);
        GameObject currentPlayerRoot = GetCharacterRoot(targetPlayerVisual);

        GameObject targetPlayerSkin = characterSO.CharacterSkin;
        GameObject targetRoot = characterSO.CharacterRoot;

        string currentSkinName = currentPlayerSkin?.name;
        string currentRootName = currentPlayerRoot?.name;

        Dictionary<string, bool> savedAllParameters = new Dictionary<string, bool>();

        if (animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    bool value = animator.GetBool(param.name);
                    savedAllParameters.Add(param.name, value);
                }
            }
            animator.avatar = characterSO.CharacterAvatar;
        }

        if (currentPlayerSkin != null)
            GameObject.Destroy(currentPlayerSkin);
        if (currentPlayerRoot != null)
            GameObject.Destroy(currentPlayerRoot);

        GameObject newSkin = GameObject.Instantiate(targetPlayerSkin, targetPlayerVisual.transform);
        GameObject newRoot = GameObject.Instantiate(targetRoot, targetPlayerVisual.transform);

        newSkin.name = currentSkinName ?? "Skin";
        newRoot.name = currentRootName ?? "Root";

        SkinnedMeshRenderer skinnedMesh = newSkin.GetComponent<SkinnedMeshRenderer>();
        Transform[] newBones = new Transform[skinnedMesh.bones.Length];
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

        foreach (Transform bone in newRoot.GetComponentsInChildren<Transform>())
        {
            boneMap[bone.name] = bone;
        }

        for (int i = 0; i < skinnedMesh.bones.Length; i++)
        {
            string boneName = skinnedMesh.bones[i].name;
            if (boneMap.ContainsKey(boneName))
            {
                newBones[i] = boneMap[boneName];
            }
        }

        skinnedMesh.bones = newBones;
        skinnedMesh.rootBone = newRoot.transform;

        skinnedMesh.enabled = false;
        await Awaitable.WaitForSecondsAsync(0.01f);
        animator.Rebind();
        animator.Update(0f);
        skinnedMesh.enabled = true;

        if (animator)
        {
            foreach (var item in savedAllParameters)
            {
                animator.SetBool(item.Key, item.Value);
            }
        }
    }
    /*
    -------------------------------------------------------
    NON STATIC METHODS
    -------------------------------------------------------
    */
    public CharacterSO GetCharacterSO(string character, Character.SearchType searchType = Character.SearchType.Default)
    {
        return AllCharactersDatabase.GetCharacter(character, searchType);
    }
    public void ChangeChracter(GameObject targetPlayerVisual, string characterName, Animator animator = null)
    {
        CharacterSO characterSO = GetCharacterSO(characterName);
        ChangeChracter(targetPlayerVisual, characterSO, animator);
    }

    /*
    -------------------------------------------------------
    NETWORK RPC METHODS
    -------------------------------------------------------
    */

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCharacterServerRpc(string character, Character.SearchType searchType = Character.SearchType.Default, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        ChangeCharacterClientRpc(senderClientId, character, searchType);
    }

    [ClientRpc]
    public void ChangeCharacterClientRpc(ulong clientId, string character, Character.SearchType searchType)
    {
        Player targetPlayer = PlayerSystem.GetPlayerByClientId(clientId);
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
