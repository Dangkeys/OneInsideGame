using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class SpawnIngredient : NetworkBehaviour
{
    [field: SerializeField] public ItemCollectionSO IngredientCollection { get; private set; }
    [field: SerializeField] public Transform SpawnPointArray {get; private set; }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int j = 0;
            for (int i = 0; i < SpawnPointArray.transform.childCount; i++)
            {
                if (j >= IngredientCollection.Ingredients.Count)
                {
                    j = 0;
                }
                
                NetworkObject ingredientPrefab = IngredientCollection.Ingredients[j];
                Transform spawnPoint = SpawnPointArray.GetChild(i);

                var instance = Instantiate(ingredientPrefab, spawnPoint.position, Quaternion.identity);

                var instanceNetworkObject = instance.GetComponent<NetworkObject>();
               instanceNetworkObject.Spawn();

                j++;
            }
        }
    }

}
