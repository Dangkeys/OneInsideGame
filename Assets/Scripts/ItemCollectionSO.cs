using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCollectionSO", menuName = "ScriptableObjects/Items")]

public class ItemCollectionSO: ScriptableObject
{
    [field: SerializeField] public List<NetworkObject> Ingredients {get; private set; }
}
