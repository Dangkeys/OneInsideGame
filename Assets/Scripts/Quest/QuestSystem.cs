using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : NetworkBehaviour
{

    private Scrollbar ScrollBar;
    [SerializeField] private Quest[] Quests;
    private NetworkVariable<int> QuestFinished = new NetworkVariable<int>(0);
    [SerializeField] private int MaxQuest;
    private List<GameObject> QuestList = new List<GameObject>();
    private NetworkVariable<List<int>> QuestNotUse = new NetworkVariable<List<int>>(new List<int>());

    private void Awake()
    {
        ScrollBar = GetComponent<Scrollbar>();
        UpdateProgressBar();
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                GameObject UseQuest = GameObject.Find(Quest.GetGameObjectName());
                QuestList.Add(UseQuest);
                Quest.SetQuestInfo(UseQuest.GetComponent<QuestInfo>());
            }
        }
    }

    private void OnEnable()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                Quest.StatusChanged += HandleQuestStatusChanged;
            }
        }
    }

    private void OnDisable()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                Quest.StatusChanged -= HandleQuestStatusChanged;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        QuestFinished.OnValueChanged += HandleQuestFinishedChanged;
        if(IsServer || IsHost)
        {
            RandomQuest();
        }
        QuestNotToSeeServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        QuestFinished.OnValueChanged -= HandleQuestFinishedChanged;
    }

    private void HandleQuestFinishedChanged(int OldValue, int NewValue)
    {
        UpdateProgressBar();
    }

    private void HandleQuestStatusChanged(bool Status)
    {
        if(IsServer)
        {
            UpdateQuestFinished(Status);
        }
    }

    private void UpdateQuestFinished(bool Status)
    {
        if (Status)
        {
            QuestFinished.Value += 1;
        }
        else
        {
            QuestFinished.Value -= 1;
        }
    }

    private void UpdateProgressBar()
    {
        if (ScrollBar != null)
        {
            float Percent = (float)QuestFinished.Value / MaxQuest;
            ScrollBar.size = Percent;
            if(Percent >= 1)
            {
                Debug.Log("Win");
            }
        }
        else
        {
            Debug.LogWarning("ScrollBar is not assigned.");
        }
    }

    private void RandomQuest()
    {
        if (QuestNotUse.Value.Count > 0)
        {
            QuestNotUse.Value.Clear();
        }
        int Amount = QuestList.Count - MaxQuest;
        List<int> AllIndex = Enumerable.Range(0, QuestList.Count).ToList();
        for (int i = 0; i < Amount; i++)
        {
            int Index = Random.Range(0, AllIndex.Count);
            QuestNotUse.Value.Add(AllIndex[Index]);
            AllIndex.RemoveAt(Index);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void QuestNotToSeeServerRpc()
    {
        if (!IsHost)
        {
            QuestNotToSee();
        }
        QuestNotToSeeClientRpc();
    }

    [ClientRpc]
    private void QuestNotToSeeClientRpc()
    {
        QuestNotToSee();
    }

    private void QuestNotToSee()
    {
        foreach (var Item in QuestNotUse.Value)
        {
            QuestList[Item].SetActive(false);
        }
    }
}
