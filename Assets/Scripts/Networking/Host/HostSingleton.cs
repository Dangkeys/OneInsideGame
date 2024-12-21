using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : SingletonPersistent<HostSingleton>
{

    public HostGameManager GameManager { get; private set; }

    public void CreateHost()
    {
        GameManager = new HostGameManager();
    }
}
