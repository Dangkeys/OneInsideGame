using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class SpawnIngredient : NetworkBehaviour
{
    [field: SerializeField] ItemCollectionSO ingredientCollection;
    [SerializeField] Item.ItemType itemToSpawn;
    private NetworkObject GetNetworkObjectFromIType(Item.ItemType itype)
    {
        switch (itype)
        {
            case Item.ItemType.IngredientA:
                return ingredientCollection.Ingredients[0];
            case Item.ItemType.IngredientB:
                return ingredientCollection.Ingredients[1];
            case Item.ItemType.IngredientC:
                return ingredientCollection.Ingredients[2];
            case Item.ItemType.IngredientD:
                return ingredientCollection.Ingredients[3];
            case Item.ItemType.IngredientE:
                return ingredientCollection.Ingredients[4];
            case Item.ItemType.ItemA:
                return ingredientCollection.Items[0];
            case Item.ItemType.ItemB:
                return ingredientCollection.Items[1];
            case Item.ItemType.ItemC:
                return ingredientCollection.Items[2];
        }
        return ingredientCollection.PlaceHolder;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkObject item = Instantiate(GetNetworkObjectFromIType(itemToSpawn), gameObject.transform.position, Quaternion.identity);
            item.Spawn();
        }
    }

}
