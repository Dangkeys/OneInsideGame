using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : NetworkBehaviour
{

    private Scrollbar ScrollBar;
    [SerializeField] private Quest[] Quests;
    private Dictionary<string, Quest> KeyAndQuests = new Dictionary<string, Quest>();
    private NetworkVariable<int> QuestFinished = new NetworkVariable<int>(0);
    private int AllQuest;

    private void Start()
    {
        ScrollBar = GetComponent<Scrollbar>();
        foreach (Quest Quest in Quests)
        {
            if(!Quest)
            {
                Debug.LogError("Quest is null");
            }
            if (KeyAndQuests.ContainsKey(Quest.Id))
            {
                Debug.Log($"Same Quest Id : {Quest.Id}");
            }
            else
            {
                KeyAndQuests[Quest.Id] = Quest;
            }
        }
        AllQuest = KeyAndQuests.Count;
        UpdateProgressBar();
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
        QuestFinished.OnValueChanged += HandleQuestFinishedChanged;

    }

    private void HandleQuestFinishedChanged(int oldValue, int newValue)
    {
        UpdateProgressBar();
    }

    private void OnDisable()
    {
        foreach (Quest Quest in Quests)
        {
            if (Quest != null)
            {
                Quest.StatusChanged += HandleQuestStatusChanged;
            }
        }
        QuestFinished.OnValueChanged -= HandleQuestFinishedChanged;
    }

    private void HandleQuestStatusChanged(bool Status)
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
            ScrollBar.size = (float)QuestFinished.Value / AllQuest;
        }
        else
        {
            Debug.LogWarning("ScrollBar is not assigned.");
        }
    }
}
