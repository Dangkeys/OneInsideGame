using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Music")]
    public SoundSO[] MainMenuMusic;

    [Header("SFX")]
    public SoundSO[] Jump;
}
