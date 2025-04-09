#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SingletonPersistentEditor
{
    [MenuItem("Tools/Debug/Reset Singletons")]
    private static void ResetSingletons()
    {
        var gameManagers = Object.FindObjectsByType<OneInsideGameManager>(FindObjectsSortMode.None);
        foreach (var manager in gameManagers)
        {
            Object.DestroyImmediate(manager.gameObject);
        }
        Debug.Log("All singletons have been reset");
    }
}
#endif