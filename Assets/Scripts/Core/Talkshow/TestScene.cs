using Unity.Netcode;
using UnityEngine;

public class TestScene : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static TestScene Instance { get; private set; }



    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Scene Started");
    }
    public override void OnNetworkSpawn()
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("Player " + player.ClientId);
        }

        Debug.Log("TestScene  Spawned on Network with ownership of  " + OwnerClientId);

        Debug.Log("Press A to increase normal number");
        Debug.Log("Press N to increase network number");
        Debug.Log("Press T to change scene ownership");
        Debug.Log("Press Y to change scene ownership without require ownership");
        Debug.Log("Press P to print all numbers"); 
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerRpc]
    public void SetOwnershipServerRpc(ulong clientId)
    {
        SetOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetOwnershipWithoutRequireOwnershipServerRpc(ulong clientId)
    {
        SetOwnership(clientId);
    }


    private void SetOwnership(ulong clientId)
    {
        if (gameObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.ChangeOwnership(clientId);
            SetOwnershipClientRpc(clientId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
        }
    }
    [ClientRpc]
    public void SetOwnershipClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("TestScene ownership changed to " + clientId);
    }

    public void PrintOwnership()
    {
        Debug.Log("TestScene ownership is " + OwnerClientId);
    }
}
