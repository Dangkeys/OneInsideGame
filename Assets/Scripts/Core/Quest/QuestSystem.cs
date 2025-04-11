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
    private NetworkVariable<List<int>> questEnable = new NetworkVariable<List<int>>(new List<int>());
    [SerializeField] private List<QuestLocation> questLocation;
    private NetworkVariable<List<int>> questChooseLocation = new NetworkVariable<List<int>>(new List<int>());

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
        UpdateQuestServerRpc();
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
        UpdateFinishedQuest(status);
    }

    private void UpdateFinishedQuest(bool status)
    {
        if (status)
        {
            finishedQuest.Value += 1;
        }
        else
        {
            if(finishedQuest.Value > 0)
            {
                finishedQuest.Value -= 1;
            }
        }
        Debug.Log(finishedQuest.Value);
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
        if (questEnable.Value.Count > 0)
        {
            questEnable.Value.Clear();
        }

        if(questChooseLocation.Value.Count > 0)
        {
            questChooseLocation.Value.Clear();
        }

        List<int> questIndex = Enumerable.Range(0, questList.Count).ToList();
        List<int> locationIndex = Enumerable.Range(0, questLocation.Count).ToList();

        for (int i = 0; i < maxQuest; i++)
        {
            int newQuestIndex = Random.Range(0, questIndex.Count);
            questEnable.Value.Add(questIndex[newQuestIndex]);
            questIndex.RemoveAt(newQuestIndex);
            int newLocationIndex = Random.Range(0, locationIndex.Count);
            questChooseLocation.Value.Add(locationIndex[newLocationIndex]);
            locationIndex.RemoveAt(newLocationIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateQuestServerRpc()
    {
        if (!IsHost)
        {
            UpdateQuest();
        }
        UpdateQuestClientRpc();
    }

    [ClientRpc]
    private void UpdateQuestClientRpc()
    {
        UpdateQuest();
    }

    private void UpdateQuest()
    {
        for (int i = 0;i < maxQuest; i++)
        {
            questList[questEnable.Value[i]].transform.position = questLocation[questChooseLocation.Value[i]].GetLocation();
            questList[questEnable.Value[i]].GetComponent<QuestInfo>().Init();
        }
    }
}
