using UnityEngine;

public class PressF : QuestInfo
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            FinishQuest();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            BrokenQuest();
        }
    }
}
