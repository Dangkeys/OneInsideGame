using UnityEngine;

public class QuestShootingDebris : QuestInfo, IInteractable
{
    [SerializeField] private ChangeCamera changeCamera;
    public void Interact(InteractionData interactionData)
    {
        if (!currentStatus)
        {
            changeCamera.SwitchCamera(0, 1);
            UpdateDoQuest(true);
        }
    }

    public override void CancelQuest()
    {
        UpdateDoQuest(false);
    }
}
