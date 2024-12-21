using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : SingletonPersistent<HostSingleton>
{

    private HostGameManager gameManager;

    public void CreateHost()
    {
        gameManager = new HostGameManager();
    }
}
