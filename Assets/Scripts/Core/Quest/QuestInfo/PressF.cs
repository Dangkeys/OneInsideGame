using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PressF : QuestInfo
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Finished(true);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            BreakQuest();
        }
    }

    private void Finished(bool finished)
    {
        FinishQuest();
        CancelQuest();
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
