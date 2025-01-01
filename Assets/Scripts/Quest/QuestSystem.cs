using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : NetworkBehaviour
{

    private Scrollbar scoreBar;
    [SerializeField] private Quest[] quests;
    private NetworkVariable<int> finishedQuest = new NetworkVariable<int>(0);
    [SerializeField] private int maxQuest;
    private List<GameObject> questList = new List<GameObject>();
    private NetworkVariable<List<int>> questDisable = new NetworkVariable<List<int>>(new List<int>());

    private void Awake()
    {
        scoreBar = GetComponent<Scrollbar>();
        UpdateProgressScoreBar();
        foreach (Quest quest in quests)
        {
            if (quest != null)
            {
                GameObject foundQuest = GameObject.Find(quest.GetGameObjectName());
                questList.Add(foundQuest);
                quest.ChangeQuestInfo(foundQuest.GetComponent<QuestInfo>());
            }
        }
    }

    private void OnEnable()
    {
        foreach (Quest quest in quests)
        {
            if (quest != null)
            {
                quest.questStatus += HandleQuestStatus;
            }
        }
    }

    private void OnDisable()
    {
        foreach (Quest quest in quests)
        {
            if (quest != null)
            {
                quest.questStatus -= HandleQuestStatus;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        finishedQuest.OnValueChanged += HandleQuestFinished;
        if(IsServer || IsHost)
        {
            RandomQuest();
        }
        DisableQuestServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        finishedQuest.OnValueChanged -= HandleQuestFinished;
    }

    private void HandleQuestFinished(int oldValue, int newValue)
    {
        UpdateProgressScoreBar();
    }

    private void HandleQuestStatus(bool status)
    {
        if(IsServer)
        {
            UpdateFinishedQuest(status);
        }
    }

    private void UpdateFinishedQuest(bool status)
    {
        if (status)
        {
            finishedQuest.Value += 1;
        }
        else
        {
            finishedQuest.Value -= 1;
        }
    }

    private void UpdateProgressScoreBar()
    {
        if (scoreBar != null)
        {
            float percent = (float)finishedQuest.Value / maxQuest;
            scoreBar.size = percent;
            if(percent >= 1)
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
        if (questDisable.Value.Count > 0)
        {
            questDisable.Value.Clear();
        }

        int amount = questList.Count - maxQuest;
        List<int> possibleIndex = Enumerable.Range(0, questList.Count).ToList();

        for (int i = 0; i < amount; i++)
        {
            int index = Random.Range(0, possibleIndex.Count);
            questDisable.Value.Add(possibleIndex[index]);
            possibleIndex.RemoveAt(index);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableQuestServerRpc()
    {
        if (!IsHost)
        {
            DisableQuest();
        }
        DisableQuestClientRpc();
    }

    [ClientRpc]
    private void DisableQuestClientRpc()
    {
        DisableQuest();
    }

    private void DisableQuest()
    {
        foreach (int index in questDisable.Value)
        {
            questList[index].SetActive(false);
        }
    }
}
