using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [field: SerializeField] public List<AbilityDataSO> OwnedAbilities { get; private set; }
}
