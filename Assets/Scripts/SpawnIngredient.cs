using UnityEngine;
using Unity.Netcode;

public class SpawnIngredient : NetworkBehaviour
{    
    public GameObject IngredientArray;
    public Transform SpawnPointArray;

    public override void OnNetworkSpawn()
    {
        SpawnIngredientServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void SpawnIngredientServerRpc()
    {
        SpawnIngredientClientRpc();
    }

    [ClientRpc]
    private void SpawnIngredientClientRpc()
    {
        int j = 0;
        for(int i = 0; i < SpawnPointArray.transform.childCount; i++)
        {
            if(j >= IngredientArray.transform.childCount)
            {
                j = 0;
            }
            GameObject ingredientPrefab = IngredientArray.transform.GetChild(j).gameObject;
            Transform spawnPoint = SpawnPointArray.GetChild(i);
            Instantiate(ingredientPrefab, spawnPoint.position, Quaternion.identity);
            j++;
        }
        
    }
}
