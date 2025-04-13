using UnityEngine;
using Unity.Netcode;

public class SpawnIngredient : NetworkBehaviour
{    
    public GameObject IngredientArray;
    public Transform SpawnPointArray;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnIngredientServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnIngredientServerRpc()
    {
        SpawnIngredientClientRpc();
    }

    [ClientRpc]
    private void SpawnIngredientClientRpc()
    {
        for(int i = 0; i < IngredientArray.transform.childCount; i++)
        {
            GameObject ingredientPrefab = IngredientArray.transform.GetChild(i).gameObject;
            Transform spawnPoint = SpawnPointArray.GetChild(i);
            Instantiate(ingredientPrefab, spawnPoint.position, Quaternion.identity);
        }
        
    }
}
