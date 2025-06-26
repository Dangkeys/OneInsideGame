using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : NetworkBehaviour
{
    private Scrollbar scoreBar;
    [SerializeField] private Quest[] quests;
    private NetworkVariable<List<bool>> finishedQuest = new NetworkVariable<List<bool>>(new List<bool>());
    [SerializeField] private int maxQuest;
    private List<GameObject> questList = new List<GameObject>();
    private NetworkVariable<List<int>> questEnable = new NetworkVariable<List<int>>(new List<int>());
    [SerializeField] private List<QuestLocation> questLocation;
    private NetworkVariable<List<int>> questChooseLocation = new NetworkVariable<List<int>>(new List<int>());

    private void Awake()
    {
        scoreBar = GetComponent<Scrollbar>();
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

    private void Init()
    {
        if (IsServer || IsHost )
        {
            if (finishedQuest.Value.Count != maxQuest)
            {
                List<bool> initial = new List<bool>(maxQuest);
                for (int i = 0; i < maxQuest; i++)
                {
                    initial.Add(false);
                }

                finishedQuest.Value = initial;
            }
            RandomQuest();
        }
        UpdateProgressScoreBar();
    }

    public override void OnNetworkSpawn()
    {
        Init();
        finishedQuest.OnValueChanged += HandleQuestFinished;
        UpdateQuestServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        finishedQuest.OnValueChanged -= HandleQuestFinished;
    }

    private void HandleQuestFinished(List<bool> previousValue, List<bool> newValue)
    {
        UpdateProgressScoreBar();
    }

    private void HandleQuestStatus(int index, bool status)
    {
        UpdateFinishedQuest(index, status);
    }

    private void UpdateFinishedQuest(int index, bool status)
    {
        List<bool> temp = new List<bool>(finishedQuest.Value);
        temp[index] = status;
        finishedQuest.Value = temp;
    }

    private void UpdateProgressScoreBar()
    {
        if (scoreBar != null)
        {
            int score = GetFinishedQuestCount();
            float percent = (float)score / maxQuest;
            scoreBar.size = percent;
            if(percent >= 1)
            {
                OneInsideLevelSystem.Instance.SetIsEndGameCollapse(true);
            }
        }
        else
        {
            Debug.LogWarning("ScrollBar is not assigned.");
        }
    }

    private int GetFinishedQuestCount()
    {
        int count = 0;
        foreach (bool isFinished in finishedQuest.Value)
        {
            if (isFinished)
                count++;
        }
        return count;
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
            questList[questEnable.Value[i]].GetComponent<QuestInfo>().Init(i);
        }
    }
}
