using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AbilityDataSO", menuName = "Scriptable Objects/Ability/AbilityDataSO", order = 0)]
public class AbilityDataSO : ScriptableObject
{
    [SerializeField, ReadOnly] private string id;
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public AbilityType Type { get; private set; }
    [field: SerializeField] public GameObject AbilityPrefab { get; private set; }

    public string Id
    {
        get
        {
            if (string.IsNullOrEmpty(id))
            {
                GenerateID();
            }
            return id;
        }
    }

    private void GenerateID()
    {
        // Only generate if it's empty
        if (!string.IsNullOrEmpty(id)) return;
        
        string timestamp = DateTime.UtcNow.Ticks.ToString("X");
        string random = UnityEngine.Random.Range(0, 1000000).ToString("D6");
        id = $"{Name?.ToUpper() ?? "ABILITY"}_{timestamp.Substring(timestamp.Length - 6)}_{random}";
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            GenerateID();
        }
    }

    public T GetAbility<T>() where T : BaseAbility
    {
        if (AbilityPrefab == null) return null;
        return AbilityPrefab.GetComponent<T>();
    }
}

#if UNITY_EDITOR
// Custom property drawer to make the field read-only in inspector
public class ReadOnlyAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif