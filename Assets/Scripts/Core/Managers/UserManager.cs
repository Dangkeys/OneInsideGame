using UnityEngine;

public class UserManager : MonoBehaviour
{
    public string CrewAbilityId { get; private set; }

    public string ImposterAbilityId { get; private set; }

    public void SetCrewAbilityId(string crewAbilityId)
    {
        //TODO : should check if the user owned this ability
        CrewAbilityId = crewAbilityId;
    }

    public void SetImposterAbilityId(string imposterAbilityId)
    {
        //TODO : should check if the user owned this ability
        ImposterAbilityId = imposterAbilityId;
    }



}