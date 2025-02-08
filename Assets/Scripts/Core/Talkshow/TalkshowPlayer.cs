using Unity.Netcode;
using UnityEngine;

public class TalkshowPlayer : NetworkBehaviour
{
    private NetworkVariable<int> networkVariableNumber = new NetworkVariable<int>();
    private int number = 0;
    private int numberWithRPC = 0;


    public override void OnNetworkSpawn()
    {

    }
    void AddNumber()
    {
        number++;
    }

    [ServerRpc]
    void AddNumberNetworkServerRpc()
    {
        networkVariableNumber.Value++;
    }
    void PrintNumber()
    {
        Debug.Log($"{OwnerClientId} : {number}");
    }
    void PrintNetworkVariableNumber()
    {
        Debug.Log($"{OwnerClientId} : {networkVariableNumber.Value}");
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (!IsOwner)
                return;
            AddNumber();
            PrintNumber();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (!IsOwner)
                return;
            AddNumberNetworkServerRpc();
            PrintNetworkVariableNumber();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!IsOwner)
                return;
            TestScene.Instance.SetOwnershipServerRpc(OwnerClientId);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Y");
            if (!IsOwner)
                return;
            TestScene.Instance.SetOwnershipWithoutRequireOwnershipServerRpc(OwnerClientId);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Normal Number");
            PrintNumber();
            Debug.Log("Network Variable Number");
            PrintNetworkVariableNumber();
        }
    }
}
