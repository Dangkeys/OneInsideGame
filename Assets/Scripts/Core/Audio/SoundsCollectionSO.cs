using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Music")]
    [field: SerializeField] public SoundSO[] MainMenuMusic { get; private set; }

    [Header("SFX")]
    [field: SerializeField] public SoundSO[] Jump {get; private set; }
}
